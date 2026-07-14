using Application.DTOs.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Board
{
    public class AddColumnRequest
    {
        [Required(ErrorMessage = "Column name cannot be empty.")]
        [MaxLength(100, ErrorMessage = "Column name cannot exceed 100 characters.")]
        public string ColumnName { get; set; } = string.Empty;
    }
    public class UpdateColumnRequest
    {
        [Required(ErrorMessage = "Column name cannot be empty.")]
        [MaxLength(100, ErrorMessage = "Column name cannot exceed 100 characters.")]
        public string NewName { get; set; } = string.Empty;
    }
    public class UpdateColumnOrderRequest
    {
        [Required]
        public List<int> OrderedColumnIds { get; set; } = new();
    }
    public class ColumnResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public List<TaskSummaryResponse> Tasks { get; set; } = new();
    }
    public class GetBoardDetailsResponse
    {
        public List<ColumnResponse> Columns { get; set; } = new();
    }
}
