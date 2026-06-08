using Core.Entities;

namespace AgileFlow.Core.Entities 
{
    public class Board : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public int ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;

        private Board() { }

        public Board(string name, int projectId)
        {
            Name = name;
            ProjectId = projectId;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }
        public ICollection<BoardColumn> BoardColumns { get; private set; } = new List<BoardColumn>();
    }

}