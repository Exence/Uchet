namespace Uchet.Classes
{
    class MainUser
    {
        public int id { get; set; }

        public int num, userId, statusId, arriveStatus;

        public string time, ch10, ch15, ch20;

        public int Num
        {
            get { return num; }
            set { num = value; }
        }
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        public int StatusId
        {
            get { return statusId; }
            set { statusId = value; }
        }
        public int ArriveStatus
        {
            get { return arriveStatus; }
            set { arriveStatus = value; }
        }
        public string Time
        {
            get { return time; }
            set { time = value; }
        }
        public string Ch10
        {
            get { return ch10; }
            set { ch10 = value; }
        }
        public string Ch15
        {
            get { return ch15; }
            set { ch15 = value; }
        }
        public string Ch20
        {
            get { return ch20; }
            set { ch20 = value; }
        }

        public MainUser() { }

        public MainUser(int userId, int num, int statusId)
        {
            this.userId = userId;
            this.num = num;
            this.statusId = statusId;
        }

    }
}
