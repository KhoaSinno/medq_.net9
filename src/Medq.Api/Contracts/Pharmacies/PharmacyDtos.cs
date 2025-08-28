using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Medq.Api.Contracts.Pharmacies
{
    public sealed class PharmacyCreateDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = default!;
        [StringLength(300)]
        public string? Address { get; set; }
        public bool OpenNow { get; set; }
    }

    public sealed class PharmacyUpdateDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = default!;
        [StringLength(300)]
        public string? Address { get; set; }
        public bool OpenNow { get; set; }
    }
}