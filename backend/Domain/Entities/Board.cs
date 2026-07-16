using Domain.Common;

namespace AgileFlow.Domain.Entities 
{
    public class Board : BaseEntity
    {
        //public string Name { get; private set; } = string.Empty;
        public int ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;

        public Board() { }

        //public void UpdateName(string name)
        //{
        //    Name = name;
        //    Update();
        //}
        public IList<BoardColumn> BoardColumns { get; private set; } = new List<BoardColumn>();
    }

}