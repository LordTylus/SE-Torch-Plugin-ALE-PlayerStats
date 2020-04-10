using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;

namespace ALE_PlayerStats {
    public class PlayerStatsPlugin : TorchPluginBase, IWpfPlugin {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private UserControl _control;
        public UserControl GetControl() => _control ?? (_control = new UserControl1());

        /// <inheritdoc />
        public override void Init(ITorchBase torch) {
            base.Init(torch);
        }
    }
}
