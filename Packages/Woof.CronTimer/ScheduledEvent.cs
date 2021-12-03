using NCrontab;
using System.Diagnostics.CodeAnalysis;

namespace Woof.Cron;

public sealed partial class CronTimer<TData> {

    /// <summary>
    /// Defines a scheduled event for <see cref="CronTimer{TData}"/>.
    /// </summary>
    public sealed class ScheduledEvent {

        /// <summary>
        /// Gets or sets the cron expression.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value is null.</exception>
        /// <exception cref="ArgumentException">Value is empty or contains invalid number of fields.</exception>
        /// <exception cref="CrontabException">Value cannot be parsed.</exception>
        public string Expression {
            get => _Expression;
            [MemberNotNull(nameof(Schedule), nameof(_Expression))]
            set {
                if (value is null) throw new ArgumentNullException(nameof(value), "Expression cannot be null");
                Schedule = CrontabSchedule.Parse(value, GetParseOptions(value));
                _Expression = value;
            }
        }

        /// <summary>
        /// Gets the data sent with the scheduler impulse.
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// Gets the parsed schedule.
        /// </summary>
        internal CrontabSchedule Schedule { get; private set; }

        /// <summary>
        /// Creates new scheduled event definition. Parses the cron expression.
        /// </summary>
        /// <param name="expression">Cron expression.</param>
        /// <param name="data">Data sent with the scheduler impulse.</param>
        /// <exception cref="ArgumentException">Thrown if expression is empty or contains invalid number of fields.</exception>
        /// <exception cref="CrontabException">Thrown when the expression fails to be parsed.</exception>
        public ScheduledEvent(string expression, TData data) {
            Expression = expression;
            Data = data;
        }

        /// <summary>
        /// Gets the <see cref="CrontabSchedule.ParseOptions"/> from expression.<br/>
        /// Pre-parses the expression to detect 5 or 6 fields format.
        /// </summary>
        /// <param name="expression">Cron expression.</param>
        /// <returns>Parse options.</returns>
        /// <exception cref="ArgumentException">Thrown if expression is empty or contains invalid number of fields.</exception>
        private static CrontabSchedule.ParseOptions GetParseOptions(string expression) {
            var fieldsCount = 0;
            var items = RxItems.Split(expression);
            if (String.IsNullOrWhiteSpace(expression)) throw new ArgumentException("The cron expression cannot be empty", nameof(expression));
            foreach (var item in items) {
                var itemFieldsCount = RxFields.Split(item).Length;
                fieldsCount = fieldsCount > 0 && itemFieldsCount != fieldsCount
                    ? throw new ArgumentException("Inconsistent field count in the cron expression", nameof(expression))
                    : itemFieldsCount;
            }
            return fieldsCount switch {
                5 => new CrontabSchedule.ParseOptions { IncludingSeconds = false },
                6 => new CrontabSchedule.ParseOptions { IncludingSeconds = true },
                _ => throw new ArgumentException("Invalid number of fields in the cron expression", nameof(expression))
            };
        }

        /// <summary>
        /// A backing field for  the <see cref="Expression"/>.
        /// </summary>
        string _Expression;

    }

}
