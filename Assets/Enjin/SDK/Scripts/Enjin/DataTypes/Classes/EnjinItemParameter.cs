namespace EnjinSDK
{
    public class EnjinItemParameter
    {
        private string _id;
        private int _index;
        private int _amount;

        private EnjinItemParameter(string id, int index, int amount)
        {
            _id = id;
            _index = index;
            _amount = amount;
        }

        public static EnjinItemParameter ConstructFungible(string id, int amount)
        {
            return new EnjinItemParameter(id, -1, amount);
        }

        public static EnjinItemParameter ConstructNonfungible(string id, int index)
        {
            return new EnjinItemParameter(id, index, 1);
        }

        public override string ToString()
        {
            if (_index != -1)
            {
                string formatString = "{{id:\"{0}\",index:\"{1}\",value:{2}}}";
                return string.Format(formatString, _id, _index, _amount);
            }
            else
            {
                string formatString = "{{id:\"{0}\",value:{1}}}";
                return string.Format(formatString, _id, _amount);
            }
        }
    }
}