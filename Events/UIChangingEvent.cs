namespace General.WPF
{
    public class UIChangingEvent
    {
        public bool Canceled { get; private set; }

        public UIChangingEvent()
        {
            this.Allow();
        }

        public void Allow()
        {
            this.Canceled = false;
        }

        public void Cancel()
        {
            this.Canceled = true;
        }
    }
}
