﻿using System.ComponentModel.DataAnnotations;

namespace AequinoctiumBot
{
    /// <summary>
    /// Genders.
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Male.
        /// </summary>
        [Display(Name = "Male")]
        Male = 0,

        /// <summary>
        /// Female.
        /// </summary>
        [Display(Name = "Female")]
        Female = 1
    }
}
