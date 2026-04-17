using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Common
{
    public class DIFormatValidation
    {
        public static DIFormatValidatorResult ValidateDIValueRule(string valInput, int ruleNo)
        {
            DIFormatValidatorResult validatorResult = new DIFormatValidatorResult();

            bool isValid = false;
            switch (ruleNo)
            {
                case 71:
                    isValid = !string.IsNullOrEmpty(valInput) && valInput.Length.Equals(2);
                    if (isValid)
                    {
                        isValid = (valInput == "FE" || valInput == "II" || valInput == "CR" || valInput == "ES" || valInput == "LP");

                        if (!isValid)
                        {
                            validatorResult.ReturnMessage = "LP";// "<< Invalid Payment Type. Please type LP if value is not available.";
                        }
                    }
                    else 
                    {
                        //Default value
                        validatorResult.ReturnMessage = "LP";
                    }
                break;                    
            }

            validatorResult.Valid = isValid;
            return validatorResult;        
        }      
    }

    public class DIFormatValidatorResult
    {
        private Boolean _valid;
        private String _returnMessage = string.Empty;

        public DIFormatValidatorResult()
        {
            _valid = true;
            _returnMessage = string.Empty;
        }

        public Boolean Valid
        {
            get
            {
                return _valid;
            }
            set
            {
                _valid = value;
            }
        }

        public String ReturnMessage
        {
            get
            {
                return _returnMessage;
            }
            set
            {
                _returnMessage = value;
            }
        }
    }
}
