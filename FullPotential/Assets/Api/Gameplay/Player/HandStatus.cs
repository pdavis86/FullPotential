using System.Collections;

namespace FullPotential.Api.Gameplay.Player
{
    public class HandStatus
    {
        public bool IsBusy { get; set; }

        public bool IsConsumingResource { get; set; }

        public IEnumerator PreActionEnumerator { get; set; }

        public IEnumerator IntraActionEnumerator { get; set; }

        public IEnumerator PostActionEnumerator { get; set; }
    }
}