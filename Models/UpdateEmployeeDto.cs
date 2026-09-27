namespace SWWebAPI.Models
{
    public class UpdateEmployeeDto
    {
        public int id { get; set; }

        public string? first_name { get; set; }

        public string? last_name { get; set; }

        public required string username { get; set; }

        public required string pswd { get; set; }

        public int? mobilenumber { get; set; }

        public string? email { get; set; }

        public int created_by { get; set; }

        public DateTime? created_date { get; set; }

        public int? modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool active { get; set; } = true;
    }
}
