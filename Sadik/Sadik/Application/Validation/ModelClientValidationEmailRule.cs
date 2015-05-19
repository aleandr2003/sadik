﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Application.Validation
{
    public class ModelClientValidationEmailRule : ModelClientValidationRule
    {
        public ModelClientValidationEmailRule(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ValidationType = "email";
        }
    }
}