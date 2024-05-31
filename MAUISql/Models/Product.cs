using SQLite;

namespace MAUISql.Models
{
    public class Note
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Content { get; set; }

        public Note Clone() => MemberwiseClone() as Note;
    }
}
