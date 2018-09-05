using AmeisenFSM;
using AmeisenFSM.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AmeisenTestConsole";

            AmeisenStateMachine stateMachine = new AmeisenStateMachine();
            stateMachine.PushAction(BotState.Idle);

            stateMachine.Update();
            Console.WriteLine($"ActiveState: {stateMachine.GetCurrentState()}");
            stateMachine.PushAction(BotState.Follow);

            stateMachine.Update();
            Console.WriteLine($"ActiveState: {stateMachine.GetCurrentState()}");
            stateMachine.PopAction();

            stateMachine.Update();
            Console.WriteLine($"ActiveState: {stateMachine.GetCurrentState()}");
            stateMachine.PopAction();

            Console.WriteLine($"ActiveState: {stateMachine.GetCurrentState()}");

            Console.ReadKey();
        }
    }
}
