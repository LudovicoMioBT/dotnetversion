using System;

namespace Elite.DotNetVersion.Domain.Entities
{
    class Epoch
    {
        public static readonly DateTime ReferralDate = new DateTime(1970, 1, 1);

        public DateTime Date { get; private set; }

        public int Number { get; private set; }

        public Epoch()
        {
            Set(DateTime.Today);
        }

        public Epoch(DateTime date)
        {
            Set(date.Date);
        }

        public Epoch(int number)
        {
            Set(number);
        }

        private void Set(DateTime date)
        {
            Date = date.Date;
            Number = (int)date.Subtract(ReferralDate).TotalDays;
        }

        private void Set(int number)
        {
            Date = ReferralDate.AddDays(number).Date;
            Number = number;
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
