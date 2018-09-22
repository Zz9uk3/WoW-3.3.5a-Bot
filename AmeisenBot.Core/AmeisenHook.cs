using AmeisenBotLogger;
using AmeisenBotUtilities;
using Magic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenBotCore
{
    /// <summary>
    /// Class that manages the hooking of WoW's EndScene
    ///
    /// !!! W.I.P !!!
    /// </summary>
    public class AmeisenHook
    {
        public bool isHooked = false;
        public bool isInjectionUsed = false;
        private uint codeCave;
        private uint codeCaveForInjection;
        private uint codeToExecute;
        private uint endsceneReturnAddress;
        private ConcurrentQueue<HookJob> hookJobs;
        private Thread hookWorker;
        private byte[] originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
        private uint returnAdress;
        private const uint ENDSCENE_HOOK_OFFSET = 0x2;
        private BlackMagic BlackMagic { get; set; }

        public AmeisenHook(BlackMagic blackmagic)
        {
            BlackMagic = blackmagic;
            Hook();
            hookJobs = new ConcurrentQueue<HookJob>();
            hookWorker = new Thread(new ThreadStart(DoWork));

            if (isHooked)
            {
                hookWorker.Start();
            }
        }

        public void AddHookJob(ref HookJob hookJob)
        {
            hookJobs.Enqueue(hookJob);
        }

        public void AddHookJob(ref ReturnHookJob hookJob)
        {
            hookJobs.Enqueue(hookJob);
        }

        public void DisposeHooking()
        {
            // Get D3D9 Endscene Pointer
            uint endscene = GetEndScene();
            endscene += ENDSCENE_HOOK_OFFSET;

            // Check if WoW is hooked
            if (BlackMagic.ReadByte(endscene) == 0xE9)
            {
                BlackMagic.WriteBytes(endscene, originalEndscene);

                BlackMagic.FreeMemory(codeCave);
                BlackMagic.FreeMemory(codeToExecute);
                BlackMagic.FreeMemory(codeCaveForInjection);
            }

            isHooked = false;
            hookWorker.Join();
        }

        private void DoWork()
        {
            while (isHooked)
            {
                if (!hookJobs.IsEmpty)
                {
                    if (hookJobs.TryDequeue(out HookJob currentJob))
                    {
                        // Process a hook job
                        InjectAndExecute(currentJob.Asm, currentJob.ReadReturnBytes);

                        // if its a chained hook job, execute it too
                        if (currentJob.GetType() == typeof(ReturnHookJob))
                        {
                            currentJob.ReturnValue = InjectAndExecute(
                                ((ReturnHookJob)currentJob).ChainedJob.Asm,
                                ((ReturnHookJob)currentJob).ChainedJob.ReadReturnBytes
                                );
                        }
                        
                        currentJob.IsFinished = true;
                    }
                }
                Thread.Sleep(5);
            }
        }

        private uint GetEndScene()
        {
            uint pDevice = BlackMagic.ReadUInt(Offsets.devicePtr1);
            uint pEnd = BlackMagic.ReadUInt(pDevice + Offsets.devicePtr2);
            uint pScene = BlackMagic.ReadUInt(pEnd);
            return BlackMagic.ReadUInt(pScene + Offsets.endScene); ;
        }

        private void Hook()
        {
            if (BlackMagic.IsProcessOpen)
            {
                // Get D3D9 Endscene Pointer
                uint endscene = GetEndScene();

                // If WoW is already hooked, unhook it
                if (BlackMagic.ReadByte(endscene) == 0xE9)
                {
                    originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
                    DisposeHooking();
                }

                try
                {
                    // If WoW is now/was unhooked, hook it
                    if (BlackMagic.ReadByte(endscene) != 0xE9)
                    {
                        // First thing thats 5 bytes big is here
                        endscene += ENDSCENE_HOOK_OFFSET;

                        // After the jump wer'e going to inject
                        endsceneReturnAddress = endscene + 0x5;

                        // Read our original EndScene
                        //originalEndscene = BlackMagic.ReadBytes(endscene, 5);

                        codeToExecute = BlackMagic.AllocateMemory(4);
                        BlackMagic.WriteInt(codeToExecute, 0);

                        returnAdress = BlackMagic.AllocateMemory(4);
                        BlackMagic.WriteInt(returnAdress, 0);

                        codeCave = BlackMagic.AllocateMemory(64);
                        codeCaveForInjection = BlackMagic.AllocateMemory(128);

                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"EndScene at: {endscene.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"EndScene returning at: {(endsceneReturnAddress).ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeCave at: {codeCave.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeCaveForInjection at: {codeCaveForInjection.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeToExecute at: {codeToExecute.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Original Endscene bytes: {Utils.ByteArrayToString(originalEndscene)}", this);

                        BlackMagic.Asm.Clear();
                        // Save registers
                        BlackMagic.Asm.AddLine("PUSHFD");
                        BlackMagic.Asm.AddLine("PUSHAD");

                        // Check for code to be executed
                        BlackMagic.Asm.AddLine($"MOV EBX, [{(codeToExecute)}]");
                        BlackMagic.Asm.AddLine("TEST EBX, 1");
                        BlackMagic.Asm.AddLine("JE @out");

                        // Execute our stuff and get return address
                        BlackMagic.Asm.AddLine($"MOV EDX, {(codeCaveForInjection)}");
                        BlackMagic.Asm.AddLine("CALL EDX");
                        BlackMagic.Asm.AddLine($"MOV [{(returnAdress)}], EAX");

                        // FInish up our execution
                        BlackMagic.Asm.AddLine("@out:");
                        BlackMagic.Asm.AddLine("MOV EDX, 0");
                        BlackMagic.Asm.AddLine($"MOV [{(codeToExecute)}], EDX");

                        // Restore registers
                        BlackMagic.Asm.AddLine("POPAD");
                        BlackMagic.Asm.AddLine("POPFD");

                        int asmLenght = BlackMagic.Asm.Assemble().Length;
                        BlackMagic.Asm.Inject(codeCave);
                        BlackMagic.Asm.Clear();

                        // Do the original EndScene stuff after we restored the registers
                        BlackMagic.WriteBytes(codeCave + (uint)asmLenght, originalEndscene);

                        // Return to original function
                        BlackMagic.Asm.AddLine($"JMP {(endsceneReturnAddress)}");
                        BlackMagic.Asm.Inject((codeCave + (uint)asmLenght) + 5);
                        BlackMagic.Asm.Clear();

                        // Modify original EndScene function to start the hook
                        BlackMagic.Asm.AddLine($"JMP {(codeCave)}");
                        BlackMagic.Asm.Inject(endscene);
                    }
                    isHooked = true;
                }
                catch { isHooked = false; }
            }
        }

        private byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            List<byte> returnBytes = new List<byte>();

            try
            {
                while (isInjectionUsed)
                {
                    Thread.Sleep(5);
                }

                isInjectionUsed = true;

                // There is code to be executed
                BlackMagic.WriteInt(codeToExecute, 1);

                // Inject the given ASM
                BlackMagic.Asm.Clear();
                foreach (string s in asm)
                {
                    BlackMagic.Asm.AddLine(s);
                }
                BlackMagic.Asm.Inject(codeCaveForInjection);

                // We don't need this atm
                //AmeisenManager.Instance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));
                //int asmLenght = BlackMagic.Asm.Assemble().Length;

                // wait for the code ti be executed
                while (BlackMagic.ReadInt(codeToExecute) > 0)
                {
                    Thread.Sleep(5);
                }

                // if we want to read the return value, do it
                if (readReturnBytes)
                {
                    byte buffer = new byte();
                    try
                    {
                        // Get our return parameter address
                        uint dwAddress = BlackMagic.ReadUInt(returnAdress);

                        // read all parameter-bytes until we the buffer is 0
                        buffer = BlackMagic.ReadByte(dwAddress);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = dwAddress + 1;
                            buffer = BlackMagic.ReadByte(dwAddress);
                        }
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log(
                            LogLevel.ERROR,
                            $"Crash at reading returnAddress: {e.ToString()}",
                            this);
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log(
                    LogLevel.ERROR,
                    $"Crash at InjectAndExecute: {e.ToString()}",
                    this);
            }

            isInjectionUsed = false;
            return returnBytes.ToArray();
        }
    }

    /// <summary>
    /// Job to execute ASM code on the endscene hook
    /// </summary>
    public class HookJob
    {
        public string[] Asm { get; set; }
        public bool IsFinished { get; set; }
        public bool ReadReturnBytes { get; set; }
        public object ReturnValue { get; set; }

        /// <summary>
        /// Build a job to execute on the endscene hook
        /// </summary>
        /// <param name="asm">ASM to execute</param>
        /// <param name="readReturnBytes">read the return bytes</param>
        public HookJob(string[] asm, bool readReturnBytes)
        {
            IsFinished = false;
            Asm = asm;
            ReadReturnBytes = readReturnBytes;
            ReturnValue = null;
        }
    }

    /// <summary>
    /// At the moment used for GetLocalizedText, to chain-execute Jobs
    /// </summary>
    public class ReturnHookJob : HookJob
    {
        public HookJob ChainedJob { get; private set; }

        /// <summary>
        /// Build a job to execute on the endscene hook
        /// </summary>
        /// <param name="asm">ASM to execute</param>
        /// <param name="readReturnBytes">read the return bytes</param>
        /// <param name="chainedJob">
        /// Job to execute after running the main Job, for example GetLocalizedText stuff
        /// </param>
        public ReturnHookJob(string[] asm, bool readReturnBytes, HookJob chainedJob) : base(asm, readReturnBytes) { ChainedJob = chainedJob; }
    }
}