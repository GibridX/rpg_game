using TextRPG.Core.Models;
using TextRPG.Core.Enums;

namespace TextRPG
{
    public class Room : GameObject, IEquatable<Room>
    {
        public RoomType Type { get; set; }
        public bool IsExplored { get; set; }
        public List<Item> Items { get; private set; }
        public string Description { get; set; }

        public Room(int x, int y, RoomType type) : base($"Комната_{x}_{y}", x, y)
        {
            Type = type;
            IsExplored = false;
            Items = new List<Item>();
            Description = GetDefaultDescription(type);
        }

        private string GetDefaultDescription(RoomType type)
        {
            return type switch
            {
                RoomType.Start => "Вы стоите у входа в древнее подземелье. Стены покрыты мхом, а воздух пахнет пылью и тайнами.",
                RoomType.Exit => "Перед вами сияющий портал выхода! Свет исходящий от него обещает свободу и спасение.",
                RoomType.Enemy => "Комната заполнена костями предыдущих авантюриеров. В воздухе витает опасность...",
                RoomType.Treasure => "Блеск золота и драгоценностей слепит глаза. Сокровищница полна богатств!",
                RoomType.Boss => "Огромное логово с костями гигантских существ. Здесь обитает нечто ужасное...",
                RoomType.Empty => "Пустая каменная комната. Тишина нарушается лишь эхом ваших шагов.",
                RoomType.Merchant => "В центре комнаты стоит сидит загадочный торговец, разложивший свои товары на разодранном покрывале.",
                RoomType.Rest => "Тихая и освещённая комната. Мягкий свет факелов и спокойная атмосфера навевают чувство безопасности.",
                _ => "Неизвестное место."
            };
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Room);
        }

        public bool Equals(Room? other)
        {
            return base.Equals(other) &&
            Type == other.Type &&
            IsExplored == other.IsExplored;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Type, IsExplored);
        }

        public override string ToString()
        {
            return $"{Type} комната на ({X}, {Y}) - Исследована: {IsExplored}";
        }
    }
}