namespace ANFW.Sample
{
    public class Character
    {
        private string _id;
        public string Id {
            get { return _id; }
            set { _id = value; }
        }
        private string _name;
        public string Name {
            get { return _name; }
            set { _name = value; }
        }
        private string _hp;
        public string Hp {
            get { return _hp; }
            set { _hp = value; }
        }
    }
}