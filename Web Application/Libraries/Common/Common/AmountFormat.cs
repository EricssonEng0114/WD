using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace UBPC.Web.Common
{
    public static class AmountFormat
    {
        public static void FormatLabel(ref Label lblAmount, decimal chqAmount)
        {
            lblAmount.Text = FormatForLabel(chqAmount);
        }

        public static string FormatForLabel(decimal chqAmount)
        {
            return String.Format("{0:n}", chqAmount);
        }

        public static void FormatTextboxLostFocus(ref TextBox txtAmount)
        {
            if (IsNumeric(txtAmount.Text.Trim()))
            {
                if (txtAmount.Text.Trim().Length > 0)
                {
                    decimal amount = Convert.ToDecimal(txtAmount.Text) / 100;

                    txtAmount.Text = String.Format("{0:n}", amount);
                }
            }
        }

        public static void FormatTextboxGotFocus(ref TextBox txtAmount)
        {
            if (txtAmount.Text.Trim().Length > 0)
            {
                if (IsNumeric(txtAmount.Text.Trim()))
                {
                    decimal amount = Convert.ToDecimal(txtAmount.Text);

                    if (amount < 1)
                        txtAmount.Text = (amount * 100).ToString().Replace(".00", "");
                    else
                        txtAmount.Text = txtAmount.Text.Replace(",", "").Replace(".", "");
                }
            }
        }

        private static bool IsNumeric(string theValue)
        {
            try
            {
                if (theValue.Length > 0)
                {
                    Convert.ToInt64(theValue);
                }
                //true if the value is numeric
                return true;
            }
            catch
            {
                //catch the error and return false - non numeric detected
                return false;
            }
        }
    }
}
