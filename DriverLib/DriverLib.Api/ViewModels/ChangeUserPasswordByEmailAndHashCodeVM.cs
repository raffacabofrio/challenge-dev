﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverLib.Api.ViewModels
{
    public class ChangeUserPasswordByHashCodeVM
    {
        public string HashCodePassword { get; set; }

        public string NewPassword { get; set; }
    }
}
