using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Medq.Api.Contracts.Clinics
{
    public sealed class ClinicCreateDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(300)]
        public string? Address { get; set; }
    }

    public sealed class ClinicUpdateDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(300)]
        public string? Address { get; set; }
    }
}