using System.Threading.Tasks;

namespace CombatTest
{
    public class AnimationTrigger : IAnimationTrigger
    {
        public async Task PlayAnimation()
        {
            await Task.Delay(1);
        }
    }
}