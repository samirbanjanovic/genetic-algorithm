using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace OnTrac.Core.WPF
{
    public class RegularExpressionValidationRule : ValidationRule
    {
        #region properties

        public string Expression { get; set; }

        public string ErrorMessage { get; set; }

        #endregion properties

        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = null;
            if (value != null)
            {
                var regEx = new Regex(Expression);
                bool isMatch = (regEx.IsMatch(value.ToString()));
                result = new ValidationResult(isMatch, isMatch ? null : ErrorMessage);
            }
            return result;
        }

        #endregion Overrides of ValidationRule
    }
}
