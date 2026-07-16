using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Domain.Entities
{
    public class BoardColumn : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public int BoardId { get; private set; }
        public Board Board { get; private set; } = null!;
        public int Position { get; private set; }

        private BoardColumn() { }

        public BoardColumn(string name, int boardId , int position)
        {
            Name = name;
            BoardId = boardId;
            Position = position;
        }
        public BoardColumn(string name, int position)
        {
            Name = name;
            Position = position;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }

        public void UpdatePosition(int newPosition)
        {
            if (newPosition < 0)
                throw new ArgumentException("Position cannot be negative.");

            Position = newPosition;
            Update();
        }

        public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();
    }
}
