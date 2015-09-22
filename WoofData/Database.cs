using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Woof.Core;

namespace Woof.Data {

    /// <summary>
    /// Universal SQL database module, version 1.1.1
    /// </summary>
    public class Database : IDisposable {

        #region Caches

        /// <summary>
        /// Connection string backup in case of dropped / broken connection
        /// </summary>
        private readonly string ConnectionString;

        /// <summary>
        /// Thread connections, one for each thread id
        /// </summary>
        private readonly Dictionary<int, SqlConnection> _Connections = new Dictionary<int, SqlConnection>();

        /// <summary>
        /// Errors collection of the last failed SQL command (if handled with internal MessageInfo handler)
        /// </summary>
        private readonly Dictionary<int, SqlErrorCollection> _LastCommandErrors = new Dictionary<int, SqlErrorCollection>();

        /// <summary>
        /// If set true all SQL messages AND ERRORS will raise events INSTEAD OF EXCEPTIONS
        /// </summary>
        private bool _GlobalFireInfoMessageEventOnUserErrors;

        /// <summary>
        /// Internal module EventLog instance
        /// </summary>
        private EventLog _EventLog;

        /// <summary>
        /// Global InfoMessage event to be triggered on SQL message or error when GlobalFireInfoMessageOnUserErrors is true
        /// </summary>
        private SqlInfoMessageEventHandler _GlobalInfoMessage;

        #endregion

        #region Regular expressions

        private static Regex RxSplitParams = new Regex(@"(?: +|, *|; *)", RegexOptions.Compiled);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when database connection loss is detected
        /// </summary>
        public static event EventHandler ConnectionLost;

        /// <summary>
        /// Occurs when all database connection are restored
        /// </summary>
        public static event EventHandler ConnectionRestored;

        /// <summary>
        /// Global InfoMessage event to be triggered on SQL message or error when GlobalFireInfoMessageOnUserErrors is true
        /// </summary>
        public event SqlInfoMessageEventHandler InfoMessage {
            add {
                _GlobalInfoMessage = value;
            }
            remove {
                foreach (var c in _Connections.Values) { c.InfoMessage -= value; c.Close(); c.Dispose(); }
                _Connections.Clear();
                _GlobalInfoMessage = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// If set true all SQL messages AND ERRORS will raise events INSTEAD OF EXCEPTIONS
        /// </summary>
        public bool FireInfoMessageEventOnUserErrors {
            get { return _GlobalFireInfoMessageEventOnUserErrors; }
            set { _GlobalFireInfoMessageEventOnUserErrors = value; foreach (var c in _Connections.Values) c.FireInfoMessageEventOnUserErrors = value; }
        }

        /// <summary>
        /// If set true, and SQL connection breaks, the program will wait indefinitely for connection to be restored
        /// USE WITH CAUTION!
        /// </summary>
        public bool RefuseNoConnection { get; set; }

        /// <summary>
        /// Current thread's open connection
        /// </summary>
        public SqlConnection Connection {
            get {
                SqlConnection c;
                int thread_id = Thread.CurrentThread.ManagedThreadId;
                lock (_Connections)
                    if (_Connections.ContainsKey(thread_id)) c = _Connections[thread_id];
                    else {
                        _Connections.Add(thread_id, c = new SqlConnection(ConnectionString));
                        c.FireInfoMessageEventOnUserErrors = _GlobalFireInfoMessageEventOnUserErrors;
                        c.InfoMessage += InternalInfoMessage;
                        if (_GlobalInfoMessage != null) c.InfoMessage += _GlobalInfoMessage;
                    }
                return c;
            }
        }

        /// <summary>
        /// Module EventLog instance, best set to configured application EventLog
        /// </summary>
        public EventLog EventLog {
            get {
                return _EventLog ?? (_EventLog = new EventLog());
            }
            set {
                _EventLog = value;
            }
        }

        /// <summary>
        /// If FireInfoMessageEventOnUserErrors is true and last command failed to complete this should contain SqlErrorCollection
        /// </summary>
        public SqlErrorCollection LastCommandErrors {
            get {
                int thread_id = Thread.CurrentThread.ManagedThreadId;
                lock (_LastCommandErrors) {
                    if (_LastCommandErrors.ContainsKey(thread_id)) return _LastCommandErrors[thread_id];
                    return null;
                }
            }
            set {
                int thread_id = Thread.CurrentThread.ManagedThreadId;
                lock (_LastCommandErrors) {
                    if (value == null) if (_LastCommandErrors.ContainsKey(thread_id)) _LastCommandErrors.Remove(thread_id);
                        else {
                            if (_LastCommandErrors.ContainsKey(thread_id)) _LastCommandErrors[thread_id] = value;
                            else _LastCommandErrors.Add(thread_id, value);
                        }
                }
            }
        }

        /// <summary>
        /// Tests whether current ConnectionString is set
        /// </summary>
        public bool IsConfigured {
            get { return !String.IsNullOrEmpty(ConnectionString); }
        }

        /// <summary>
        /// Time before command should be terminated with an exception
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseHardConnectionTimeLimit { get; set; }

        #endregion

        #region Constants

        private const string MsgExNoData = "Record contains no usable properties or fields";

        #endregion

        #region Core methods

        /// <summary>
        /// Initializes SQL module for specified connection string
        /// </summary>
        public Database(string connectionString) {
            ConnectionString = connectionString;
            CommandTimeout = 7200; // 2h, let it complete :)
            UseHardConnectionTimeLimit = true;
        }

        /// <summary>
        /// Sets error collection for current thread's connection if command failed to complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InternalInfoMessage(object sender, SqlInfoMessageEventArgs e) {
            foreach (SqlError i in e.Errors) if (i.Class > 10) LastCommandErrors = e.Errors;
        }

        #endregion Core methods

        #region Data retrieval methods

        /// <summary>
        /// Executes stored procedure and ignores its effects
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching to parameters</param>
        /// <param name="output">object with properties matching to output parameters</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public int Exec(string proc, object parameters = null, object output = null) {
            using (var command = GetCommand(proc, parameters, output)) {
                LastCommandErrors = null;
                int rowsAffected = DispatchExecuteNonQuery(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                if (output != null) SetOutputParameters(command, output);
                return rowsAffected;
            }
        }

        /// <summary>
        /// Executes stored procedure and ignores its effects
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public int ExecP(string signature, params object[] parameters) {
            using (var command = GetCommand(signature, parameters)) {
                LastCommandErrors = null;
                int rowsAffected = DispatchExecuteNonQuery(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                
                return rowsAffected;
            }
        }

        /// <summary>
        /// Executes stored procedure and returns its return value as Scalar (int/bool)
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching to parameters</param>
        /// <param name="output">object with properties matching to output parameters</param>
        /// <returns>int/bool</returns>
        public Scalar GetReturn(string proc, object parameters = null, object output = null) {
            using (var command = GetCommand(proc, parameters, output)) {
                LastCommandErrors = null;
                var retParam = command.Parameters.Add("RetVal", SqlDbType.Int);
                retParam.Direction = ParameterDirection.ReturnValue;
                DispatchExecuteNonQuery(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                if (output != null) SetOutputParameters(command, output);
                return new Scalar(retParam.Value);
            }
        }

        /// <summary>
        /// Executes stored procedure and returns its return value as Scalar (int/bool)
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns>int/bool</returns>
        public Scalar GetReturnP(string signature, params object[] parameters) {
            using (var command = GetCommand(signature, parameters)) {
                LastCommandErrors = null;
                var retParam = command.Parameters.Add("RetVal", SqlDbType.Int);
                retParam.Direction = ParameterDirection.ReturnValue;
                DispatchExecuteNonQuery(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                return new Scalar(retParam.Value);
            }
        }

        /// <summary>
        /// Executes stored procedure and returns selected scalar value
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching to parameters</param>
        /// <param name="output">object with properties matching to output parameters</param>
        /// <returns>Scalar</returns>
        public Scalar GetScalar(string proc, object parameters = null, object output = null) {
            Scalar value;
            using (var command = GetCommand(proc, parameters, output)) {
                LastCommandErrors = null;
                value = DispatchExecuteScalar(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                if (output != null) SetOutputParameters(command, output);
                return value;
            }
        }

        /// <summary>
        /// Executes stored procedure and returns selected scalar value
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns>Scalar</returns>
        public Scalar GetScalarP(string signature, params object[] parameters) {
            Scalar value;
            using (var command = GetCommand(signature, parameters)) {
                LastCommandErrors = null;
                value = DispatchExecuteScalar(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                return value;
            }
        }

        /// <summary>
        /// Executes stored procedure and returns a DataSet
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns></returns>
        public DataSet GetDataSet(string proc, object parameters = null, object output = null) {
            DataSet dataSet = new DataSet();
            using (var command = GetCommand(proc, parameters, output)) {
                LastCommandErrors = null;
                //ConnectionOpen();
                using (var adapter = new SqlDataAdapter(command)) adapter.Fill(dataSet);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                if (output != null) SetOutputParameters(command, output);
            }
            return dataSet;
        }

        /// <summary>
        /// Executes stored procedure and returns a DataSet
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public DataSet GetDataSetP(string signature, params object[] parameters) {
            DataSet dataSet = new DataSet();
            using (var command = GetCommand(signature, parameters)) {
                LastCommandErrors = null;
                //ConnectionOpen();
                using (var adapter = new SqlDataAdapter(command)) adapter.Fill(dataSet);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
            }
            return dataSet;
        }


        /// <summary>
        /// Executes stored procedure and returns an object containing lists of strong-typed records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns></returns>
        public T GetDataSetAs<T>(string proc, object parameters = null, object output = null) {
            return CastDataSet<T>(GetDataSet(proc, parameters, output));
        }

        /// <summary>
        /// Executes stored procedure and returns an object containing lists of strong-typed records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public T GetDataSetPAs<T>(string signature, params object[] parameters) {
            return CastDataSet<T>(GetDataSetP(signature, parameters));
        }

        /// <summary>
        /// Executes stored procedure and returns SqlDataReader
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns>Scalar</returns>
        public SqlDataReader GetReader(string proc, object parameters = null, object output = null) {
            SqlDataReader reader;
            using (var command = GetCommand(proc, parameters, output)) {
                LastCommandErrors = null;
                reader = DispatchExecuteReader(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                if (output != null) SetOutputParameters(command, output);
                return reader;
            }
        }

        /// <summary>
        /// Executes stored procedure and returns SqlDataReader
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns>Scalar</returns>
        public SqlDataReader GetReaderP(string signature, params object[] parameters) {
            SqlDataReader reader;
            using (var command = GetCommand(signature, parameters)) {
                LastCommandErrors = null;
                reader = DispatchExecuteReader(command);
                if (LastCommandErrors != null) throw new CommandFailedException(LastCommandErrors);
                return reader;
            }
        }

        /// <summary>
        /// Reads a data record from SqlDataReader and casts it to the specified record class
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <returns>data record</returns>
        public T ReadAs<T>(SqlDataReader reader) {
            var t = typeof(T);
            var stringName = typeof(String).Name;
            if (t.IsPrimitive || t.Name == stringName) return (T)reader.GetValue(0);
            var length = reader.FieldCount;
            var values = new object[length];
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var vMap = new Dictionary<string, object>();
            var record = default(T);
            object value;
            reader.GetValues(values);
            for (var i = 0; i < values.Length; i++) vMap[reader.GetName(i)] = values[i];
            if (t.IsValueType) {
                var boxed = (object)record;
                foreach (var field in fields) {
                    value = vMap[field.Name];
                    if (value is DateTime) {
                        var dt = (DateTime)value;
                        if (dt <= SqlDateTime.MinValue) dt = DateTime.MinValue;
                        else if (dt >= SqlDateTime.MaxValue) dt = DateTime.MaxValue;
                        value = dt;
                    }
                    if (!(value is DBNull)) field.SetValue(boxed, value);
                }
                record = (T)boxed;
            }
            else
                if (record == null) record = (T)Activator.CreateInstance(t); // if record type is class, not struct
                foreach (var field in fields) {
                    value = vMap[field.Name];
                    if (value is DateTime) {
                        var dt = (DateTime)value;
                        if (dt <= SqlDateTime.MinValue) dt = DateTime.MinValue;
                        else if (dt >= SqlDateTime.MaxValue) dt = DateTime.MaxValue;
                        value = dt;
                    }
                    if (!(value is DBNull)) field.SetValue(record, value);
                }
            return record;
        }

        /// <summary>
        /// Executes stored procedure and returns a single data record
        /// </summary>
        /// <typeparam name="T">record type</typeparam>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns>single data record</returns>
        public T GetRecord<T>(string proc, object parameters = null, object output = null) {
            var record = default(T);
            using (var reader = GetReader(proc, parameters, output)) if (reader.Read()) record = ReadAs<T>(reader);
            return record;
        }


        /// <summary>
        /// Executes stored procedure and returns a single data record
        /// </summary>
        /// <typeparam name="T">record type</typeparam>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns>single data record</returns>
        public T GetRecordP<T>(string signature, params object[] parameters) {
            var record = default(T);
            using (var reader = GetReaderP(signature, parameters)) if (reader.Read()) record = ReadAs<T>(reader);
            return record;
        }

        /// <summary>
        /// Executes stored procedure and returns a list of specified type
        /// </summary>
        /// <typeparam name="T">list type</typeparam>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns>Scalar</returns>
        public List<T> GetList<T>(string proc, object parameters = null, object output = null) {
            var list = new List<T>();
            using (var reader = GetReader(proc, parameters, output)) while (reader.Read()) list.Add(ReadAs<T>(reader));
            return list;
        }

        /// <summary>
        /// Executes stored procedure and returns a list of specified type
        /// </summary>
        /// <typeparam name="T">list type</typeparam>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns>Scalar</returns>
        public List<T> GetListP<T>(string signature, params object[] parameters) {
            var list = new List<T>();
            using (var reader = GetReaderP(signature, parameters)) while (reader.Read()) list.Add(ReadAs<T>(reader));
            return list;
        }

        /// <summary>
        /// Executes stored procedure and returns a list of specified type
        /// populated with limited number of records
        /// </summary>
        /// <typeparam name="T">list type</typeparam>
        /// <param name="count">results limit</param>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching parameters</param>
        /// <returns></returns>
        public List<T> GetList<T>(int count, string proc, object parameters) {
            var list = new List<T>();
            int i = 0;
            using (var reader = GetReader(proc, parameters)) while (i++ < count && reader.Read()) list.Add(ReadAs<T>(reader));
            return list;
        }

        /// <summary>
        /// Executes stored procedure and returns a list of specified type
        /// populated with limited number of records
        /// </summary>
        /// <typeparam name="T">list type</typeparam>
        /// <param name="count">results limit</param>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public List<T> GetListP<T>(int count, string signature, params object[] parameters) {
            var list = new List<T>();
            int i = 0;
            using (var reader = GetReaderP(signature, parameters)) while (i++ < count && reader.Read()) list.Add(ReadAs<T>(reader));
            return list;
        }

        #endregion Data retrieval methods

        #region Command dispatcher


        /// <summary>
        /// Executes command and returns affected rows number
        /// Retries indefinitely on connection errors, if ThrowExceptionOnConnectionError is not set
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public int DispatchExecuteNonQuery(SqlCommand command) {
            if (command.Connection.State != ConnectionState.Open) command.Connection.Open();
            var result = command.ExecuteNonQuery();
            ConnectionDown = false;
            return result;
        }

        /// <summary>
        /// Executs command and returns first row's first cell's value as scalar
        /// Retries indefinitely on connection errors, if ThrowExceptionOnConnectionError is not set
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Scalar DispatchExecuteScalar(SqlCommand command) {
            var resultObject = command.ExecuteScalar();
            var result = new Scalar(resultObject);
            ConnectionDown = false;
            return result;
        }

        /// <summary>
        /// Executs command and returns SqlDataReader
        /// Retries indefinitely on connection errors, if ThrowExceptionOnConnectionError is not set
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public SqlDataReader DispatchExecuteReader(SqlCommand command) {
            var result = command.ExecuteReader();
            ConnectionDown = false;
            return result;
        }

        #endregion

        #region Bullet-proof connection handling

        /// <summary>
        /// True if current connection with database is lost
        /// </summary>
        private static bool _ConnectionDown;
        /// <summary>
        /// A list of thread ids in which connection lost occured
        /// </summary>
        private static List<int> _ConnectionDownThreads = new List<int>();

        /// <summary>
        /// A property set true when connection is lost in first thread, and set false when it's restored in last thread
        /// Triggers ConnectionLost and ConnectionRestored events when applicable
        /// </summary>
        private static bool ConnectionDown {
            get {
                return _ConnectionDown;
            }
            set {
                if (value) {
                    if (_ConnectionDownThreads.Count == 0) {
                        if (!_ConnectionDownThreads.Contains(Thread.CurrentThread.ManagedThreadId)) {
                            _ConnectionDownThreads.Add(Thread.CurrentThread.ManagedThreadId);
                            if (ConnectionLost != null) ConnectionLost.Invoke(null, EventArgs.Empty);
                            Debug.Print("!!! Connection down detected.");
                        }
                    }
                }
                else if (_ConnectionDownThreads.Count > 0) {
                    _ConnectionDownThreads.Remove(Thread.CurrentThread.ManagedThreadId);
                    if (_ConnectionDownThreads.Count < 1) {
                        _ConnectionDown = value;
                        if (ConnectionRestored != null) ConnectionRestored.Invoke(null, EventArgs.Empty);
                        Debug.Print("!!! Connection restored.");
                    }
                }
            }
        }

        /// <summary>
        /// Resets thread's broken connection instance
        /// </summary>
        private bool ConnectionReset() {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            lock (_Connections) {
                foreach (var c in _Connections) { c.Value.Close(); c.Value.Dispose(); }
                _Connections.Clear();
                Thread.Sleep(1000);
                var connection = new SqlConnection(ConnectionString);
                _Connections.Add(threadId, connection);
                var cts = new CancellationTokenSource(connection.ConnectionTimeout * 1000);
                Task t = null;
                try { t = connection.OpenAsync(cts.Token); } catch { }
                if (t != null) Task.WaitAny(t);
                return connection.State == ConnectionState.Open;
            }
        }

        /// <summary>
        /// Returns true if exception given is a connection error
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool IsConnectionError(SqlException x) {
            return ConnectionDown =
                (x.Message.Contains("network-related") || x.Message.Contains("transport-level"));
        }

        #endregion

        #region Internal command builder helpers and data converters

        delegate void CommandProcessorDelegate(SqlCommand command);

        /// <summary>
        /// Creates SQL command and opens a connection for it
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="setParameters">a delegate setting optional input and output parameteres</param>
        /// <returns></returns>
        private SqlCommand GetCommand(string proc, CommandProcessorDelegate setParameters) {
            SqlCommand command = null;
            int restoreAttempts = 3;
            retry:
            // Make a fresh command:
            if (command != null) { command.Dispose(); command = null; GC.Collect(); }
            command = new SqlCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = proc;
            if (setParameters != null) setParameters(command);   //AddParameters(command, parameters);
            command.CommandTimeout = CommandTimeout;
            // Ensure there's a connection for it
            SqlConnection c = Connection;
            if (c.State != ConnectionState.Open) { // if it's not open
                var cts = new CancellationTokenSource(c.ConnectionTimeout * 1000);
                try {
                    if (UseHardConnectionTimeLimit) { // async mode to assert timeout properly
                        Task t = c.OpenAsync(cts.Token);
                        Task.WaitAny(t);
                        if (c.State != ConnectionState.Open && cts.IsCancellationRequested) {
                            ConnectionDown = true;
                            if (RefuseNoConnection) {
                                if (!ConnectionReset()) goto retry;
                            }
                            else throw new TimeoutException();
                        }
                    } else { // sync mode, timeout will not be respected
                        c.Open();
                        if (c.State != ConnectionState.Open) {
                            ConnectionDown = true;
                            if (RefuseNoConnection) {
                                if (!ConnectionReset()) goto retry;
                            }
                        }
                    }
                } catch (InvalidOperationException) {
                    ConnectionDown = true;
                    if (RefuseNoConnection) {
                        if (!ConnectionReset()) goto retry;
                    }
                    else {
                        if (!ConnectionReset()) throw;
                    }
                } catch (SqlException x) {
                    ConnectionDown = true;
                    if (!IsConnectionError(x)) throw;
                    if (RefuseNoConnection) {
                        if (!ConnectionReset()) goto retry;
                    }
                    else {
                        if (!ConnectionReset()) throw;
                    }
                }
            }
            command.Connection = c;
            if (c.State != ConnectionState.Open) {
                ConnectionDown = true;
                if (RefuseNoConnection) goto retry;
                else if (!ConnectionReset()) {
                    if (--restoreAttempts > 0) { Thread.Sleep(c.ConnectionTimeout * 1000); goto retry; }
                    throw new InvalidOperationException("Couldn't open SQL connection.");
                }
            }
            return command;
        }

        /// <summary>
        /// Creates SQL command and opens a connection for it
        /// </summary>
        /// <param name="proc">stored procedure name</param>
        /// <param name="parameters">object with properties matching input parameters</param>
        /// <param name="output">object with properties matching output parameters</param>
        /// <returns></returns>
        private SqlCommand GetCommand(string proc, object parameters = null, object output = null) {
            return GetCommand(
                proc,
                new CommandProcessorDelegate((command) => {
                    if (parameters != null) AddParameters(command, parameters);
                    if (output != null) AddOutputParameters(command, output);
                })
            );
        }

        /// <summary>
        /// Creates SQL command and opens a connection for it
        /// </summary>
        /// <param name="signature">stored procedure signature</param>
        /// <param name="parameters">input parameters</param>
        /// <returns></returns>
        private SqlCommand GetCommand(string signature, object[] parameters) {
            int i1 = signature.IndexOf('('), i2 = signature.IndexOf(')');
            if (i1 < 0 || i2 < 0 || i1 >= i2) throw new ArgumentException("Use proc(a,b,c...) syntax", "");
            var procName = signature.Substring(0, i1);
            var parameterNames = RxSplitParams.Split(signature.Substring(i1 + 1, i2 - i1 - 1));
            return GetCommand(
                procName,
                new CommandProcessorDelegate((command) => {
                    AddParameters(command, parameterNames, parameters);
                })
            );
        }

        /// <summary>
        /// Adds properties from a specified object to given command as parameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        private void AddParameters(SqlCommand command, object parameters) {
            var m = new XMap(parameters);
            m.ForEach(d => command.Parameters.AddWithValue('@' + d.Key, new Convert(d.Value, m.Types[d.Key]).Value));
        }

        /// <summary>
        /// Adds parameters to the command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="names"></param>
        /// <param name="parameters"></param>
        private void AddParameters(SqlCommand command, string[] names, object[] parameters) {
            for (int i = 0; i < names.Length; i++)
                command.Parameters.AddWithValue(names[i], new Convert(parameters[i]).Value);            
        }

        /// <summary>
        /// Adds properties from a specified object to given command as output parameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="output">Boxed output structure</param>
        private void AddOutputParameters(SqlCommand command, object output) {
            var m = new XMap(output);
            m.ForEach(d => command.Parameters.AddWithValue('@' + d.Key, d.Value ?? DBNull.Value).Direction = ParameterDirection.Output);
        }

        /// <summary>
        /// Assigns output values to output values object
        /// </summary>
        /// <param name="c"></param>
        /// <param name="output">Boxed output structure</param>
        private void SetOutputParameters(SqlCommand c, object output) {
            var f = new List<FieldInfo>(output.GetType().GetFields());
            foreach (SqlParameter p in c.Parameters) {
                if (p.Direction == ParameterDirection.Output) {
                    var n = p.ParameterName.Substring(1);
                    f.Find(i => i.Name == n).SetValue(output, p.Value);
                }
            }
        }

        /// <summary>
        /// Converts SQL-type object to regular .NET object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object FromSql(object value) {
            if (value is DBNull) return null;
            if (value is DateTime) {
                var dt = (DateTime)value;
                if (dt <= SqlDateTime.MinValue) dt = DateTime.MinValue;
                else if (dt >= SqlDateTime.MaxValue) dt = DateTime.MaxValue;
                return dt;
            }
            return value;
        }

        /// <summary>
        /// Cast an array of objects to specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private object CastToFields(Type type, object[] values) {
            var fields = type.GetFields();
            var instance = Activator.CreateInstance(type);
            for (int i = 0; i < fields.Length; i++) fields[i].SetValue(instance, FromSql(values[i]));
            return instance;
        }

        /// <summary>
        /// Cast an array of objects to specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        private T CastToFields<T>(object[] values) {
            var type = typeof(T);
            var fields = type.GetFields();
            if (type.IsValueType) { // type is struct
                var boxed = (object)(default(T));
                for (int i = 0; i < fields.Length; i++) fields[i].SetValue(boxed, FromSql(values[i]));
                return (T)boxed;
            } else { // type is class
                var data = default(T);
                for (int i = 0; i < fields.Length; i++) fields[i].SetValue(data, FromSql(values[i]));
                return data;
            }
        }

        /// <summary>
        /// Casts a DataSet to a type containing lists of strong-typed records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private T CastDataSet<T>(DataSet data) {
            var type = typeof(T);
            var valueType = type.IsValueType;
            var cast = default(T);
            object boxed = null;
            var fields = type.GetFields();
            if (cast == null) cast = (T)Activator.CreateInstance(type);
            if (valueType) boxed = (object)cast;
            for (int i = 0; i < fields.Length; i++) {
                if (i < data.Tables.Count) {
                    var itemType = (fields[i].FieldType.GetDefaultMembers()[0] as PropertyInfo).PropertyType;
                    var list = Activator.CreateInstance(fields[i].FieldType) as IList;
                    var rows = data.Tables[i].Rows;
                    foreach (DataRow row in rows) {
                        var item = CastToFields(itemType, row.ItemArray);
                        list.Add(item);
                    }
                    if (valueType) fields[i].SetValue(boxed, list); else fields[i].SetValue(cast, list);
                } else break;
            }
            if (valueType) cast = (T)boxed;
            return cast;
        }

        #endregion

        #region IDisposable

        public bool IsDisposed { get; set; }

        ~Database() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                foreach (var i in _Connections) {
                    while (i.Value.State != ConnectionState.Closed) { // trying to dispose a connection with busy command would crash the app!
                        Thread.Sleep(20);
                        if (i.Value.State == ConnectionState.Open) i.Value.Close();
                    }
                    i.Value.Dispose();
                }
                _Connections.Clear();
            }
            IsDisposed = true;
        }

        #endregion
    
    }

}