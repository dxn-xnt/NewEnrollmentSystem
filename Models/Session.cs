﻿using System;
using System.ComponentModel.DataAnnotations;

namespace EnrollmentSystem.Models;

public class Session
{
    public int SsnId { get; set; }
    
    [Required]
    public TimeSpan TslStartTime { get; set; }
    
    [Required]
    public TimeSpan TslEndTime { get; set; }
    
    [Required]
    public int TslDay { get; set; }
    
    [Required]
    public int SchdId { get; set; }
}