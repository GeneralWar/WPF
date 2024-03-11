using System;

namespace General.WPF
{
    public interface IComponentHostedData
    {
        void onLoad(object sender, EventArgs e);
        void onUnload(object sender, EventArgs e);
    }
}
