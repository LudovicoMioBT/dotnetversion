using System;

namespace Elite.DotNetVersion
{
    class Epoch
    {
        public static readonly DateTime ReferralDate = new DateTime(1970, 1, 1);

        public DateTime Date { get; private set; }

        public int Number { get; private set; }

        public Epoch()
        {
            this.Set(DateTime.Today);
        }

        public Epoch(DateTime date)
        {
            this.Set(date.Date);
        }

        public Epoch(int number)
        {
            this.Set(number);
        }

        private void Set(DateTime date)
        {
            this.Date = date.Date;
            this.Number = (int)date.Subtract(Epoch.ReferralDate).TotalDays;
        }

        private void Set(int number)
        {
            this.Date = Epoch.ReferralDate.AddDays(number).Date;
            this.Number = number;
        }

        public static implicit operator DateTime(Epoch date)
        {
            return date.Date;
        }

        public static implicit operator int(Epoch date)
        {
            return date.Number;
        }

        public static implicit operator Epoch(DateTime date)
        {
            return new Epoch(date);
        }

        public static implicit operator Epoch(int number)
        {
            return new Epoch(number);
        }
    }
}
