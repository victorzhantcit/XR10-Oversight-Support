namespace Unity.Extensions
{
    using System;
    using System.Globalization;

    public static class TimeConvert
    {
        private const string DEFAULT_FORMAT = "yyyy-MM-dd";

        // �榡�� DateTime ���r��
        public static string ToFormattedString(DateTime dateTime, string format = DEFAULT_FORMAT)
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentNullException(nameof(format), "Format cannot be null or empty.");
            }
            return dateTime.ToString(format);
        }

        // �N�榡�Ʀr���ഫ�� DateTime
        public static DateTime ToDateTime(string formattedDate, string targetFormat = DEFAULT_FORMAT, bool throwOnError = true)
        {
            if (string.IsNullOrEmpty(targetFormat))
            {
                throw new ArgumentNullException(nameof(targetFormat), "Target format cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(formattedDate))
            {
                throw new ArgumentNullException(nameof(formattedDate), "Formatted date string cannot be null or empty.");
            }

            if (DateTime.TryParseExact(
                    formattedDate,
                    targetFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime dateTime))
            {
                return dateTime;
            }

            if (throwOnError)
            {
                throw new ArgumentException($"Invalid date format: {formattedDate}. Expected format: {targetFormat}");
            }

            return DateTime.MinValue;
        }

        // �䴩�h�Ӯ榡������ѪR
        public static DateTime ToDateTime(string formattedDate, string[] targetFormats, bool throwOnError = true)
        {
            if (targetFormats == null || targetFormats.Length == 0)
            {
                throw new ArgumentNullException(nameof(targetFormats), "Target formats cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(formattedDate))
            {
                throw new ArgumentNullException(nameof(formattedDate), "Formatted date string cannot be null or empty.");
            }

            if (DateTime.TryParseExact(
                    formattedDate,
                    targetFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime dateTime))
            {
                return dateTime;
            }

            if (throwOnError)
            {
                throw new ArgumentException($"Invalid date format: {formattedDate}. Expected formats: {string.Join(", ", targetFormats)}");
            }

            return DateTime.MinValue;
        }

        // ���o��쪺���
        public static DateTime GetFirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }
    }

}

