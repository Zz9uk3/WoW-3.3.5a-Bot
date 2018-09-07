using AmeisenBotLogger;
using AmeisenBotUtilities;
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

        private static readonly object padlock = new object();

        private static AmeisenHook instance;

        private uint codeCave;

        private uint codeCaveForInjection;

        private uint codeToExecute;

        private uint endsceneReturnAddress;

        private ConcurrentQueue<HookJob> hookJobs;

        private Thread hookWorker;

        private byte[] originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };

        private uint returnAdress;

        public static AmeisenHook Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AmeisenHook();
                    }

                    return instance;
                }
            }
        }

        private AmeisenHook()
        {
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

            uint endsceneHookOffset = 0x2;
            endscene += endsceneHookOffset;

            // Check if WoW is hooked
            if (AmeisenCore.BlackMagic.ReadByte(endscene) == 0xE9)
            {
                AmeisenCore.BlackMagic.WriteBytes(endscene, originalEndscene);

                AmeisenCore.BlackMagic.FreeMemory(codeCave);
                AmeisenCore.BlackMagic.FreeMemory(codeToExecute);
                AmeisenCore.BlackMagic.FreeMemory(codeCaveForInjection);
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
                        InjectAndExecute(currentJob.Asm, currentJob.ReadReturnBytes);

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
                Thread.Sleep(1);
            }
        }

        private uint GetEndScene()
        {
            uint pDevice = AmeisenCore.BlackMagic.ReadUInt(Offsets.devicePtr1);
            uint pEnd = AmeisenCore.BlackMagic.ReadUInt(pDevice + Offsets.devicePtr2);
            uint pScene = AmeisenCore.BlackMagic.ReadUInt(pEnd);
            uint endscene = AmeisenCore.BlackMagic.ReadUInt(pScene + Offsets.endScene);
            string debug = endscene.ToString("X");
            return endscene;
        }

        private void Hook()
        {
            if (AmeisenCore.BlackMagic.IsProcessOpen)
            {
                // Get D3D9 Endscene Pointer
                uint endscene = GetEndScene();

                // If WoW is already hooked, unhook it
                if (AmeisenCore.BlackMagic.ReadByte(endscene) == 0xE9)
                {
                    originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
                    DisposeHooking();
                }

                try
                {
                    // If WoW is now/was unhooked, hook it
                    if (AmeisenCore.BlackMagic.ReadByte(endscene) != 0xE9)
                    {
                        uint endsceneHookOffset = 0x2;
                        endscene += endsceneHookOffset;

                        endsceneReturnAddress = endscene + 0x5;

                        //originalEndscene = AmeisenManager.Instance().GetBlackMagic().ReadBytes(endscene, 5);

                        codeToExecute = AmeisenCore.BlackMagic.AllocateMemory(4);
                        AmeisenCore.BlackMagic.WriteInt(codeToExecute, 0);

                        returnAdress = AmeisenCore.BlackMagic.AllocateMemory(4);
                        AmeisenCore.BlackMagic.WriteInt(returnAdress, 0);

                        codeCave = AmeisenCore.BlackMagic.AllocateMemory(32);
                        codeCaveForInjection = AmeisenCore.BlackMagic.AllocateMemory(64);

                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"EndScene at: {endscene.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"EndScene returning at: {(endsceneReturnAddress).ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeCave at: {codeCave.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeCaveForInjection at: {codeCaveForInjection.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"CodeToExecute at: {codeToExecute.ToString("X")}", this);
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Original Endscene bytes: {Utils.ByteArrayToString(originalEndscene)}", this);

                        AmeisenCore.BlackMagic.WriteBytes(codeCave, originalEndscene);

                        AmeisenCore.BlackMagic.Asm.Clear();
                        AmeisenCore.BlackMagic.Asm.AddLine("PUSHFD");
                        AmeisenCore.BlackMagic.Asm.AddLine("PUSHAD");
                        AmeisenCore.BlackMagic.Asm.AddLine($"MOV EBX, [{(codeToExecute)}]");
                        AmeisenCore.BlackMagic.Asm.AddLine("TEST EBX, 1");
                        AmeisenCore.BlackMagic.Asm.AddLine("JE @out");

                        AmeisenCore.BlackMagic.Asm.AddLine($"MOV EDX, {(codeCaveForInjection)}");
                        AmeisenCore.BlackMagic.Asm.AddLine("CALL EDX");
                        AmeisenCore.BlackMagic.Asm.AddLine($"MOV [{(returnAdress)}], EAX");

                        AmeisenCore.BlackMagic.Asm.AddLine("@out:");
                        AmeisenCore.BlackMagic.Asm.AddLine("MOV EDX, 0");
                        AmeisenCore.BlackMagic.Asm.AddLine($"MOV [{(codeToExecute)}], EDX");

                        AmeisenCore.BlackMagic.Asm.AddLine("POPAD");
                        AmeisenCore.BlackMagic.Asm.AddLine("POPFD");
                        int asmLenght = AmeisenCore.BlackMagic.Asm.Assemble().Length;
                        AmeisenCore.BlackMagic.Asm.Inject(codeCave + 5);

                        AmeisenCore.BlackMagic.Asm.Clear();
                        AmeisenCore.BlackMagic.Asm.AddLine($"JMP {(endsceneReturnAddress)}");
                        AmeisenCore.BlackMagic.Asm.Inject((codeCave + (uint)asmLenght) + 5);

                        AmeisenCore.BlackMagic.Asm.Clear();
                        AmeisenCore.BlackMagic.Asm.AddLine($"JMP {(codeCave)}");
                        AmeisenCore.BlackMagic.Asm.Inject(endscene);
                    }
                    isHooked = true;
                }
                catch { isHooked = false; }
            }
        }

        private byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            try
            {
                while (isInjectionUsed)
                {
                    Thread.Sleep(1);
                }

                isInjectionUsed = true;

                AmeisenCore.BlackMagic.WriteInt(codeToExecute, 1);
                AmeisenCore.BlackMagic.Asm.Clear();

                if (asm != null)
                {
                    foreach (string s in asm)
                    {
                        AmeisenCore.BlackMagic.Asm.AddLine(s);
                    }
                }

                //AmeisenManager.Instance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));

                int asmLenght = AmeisenCore.BlackMagic.Asm.Assemble().Length;
                AmeisenCore.BlackMagic.Asm.Inject(codeCaveForInjection);

                while (AmeisenCore.BlackMagic.ReadInt(codeToExecute) > 0)
                {
                    Thread.Sleep(1);
                }

                if (readReturnBytes)
                {
                    byte buffer = new byte();
                    List<byte> returnBytes = new List<byte>();

                    try
                    {
                        uint dwAddress = AmeisenCore.BlackMagic.ReadUInt(returnAdress);

                        buffer = AmeisenCore.BlackMagic.ReadByte(dwAddress);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = dwAddress + 1;
                            buffer = AmeisenCore.BlackMagic.ReadByte(dwAddress);
                        }
                    }
                    catch (Exception e) { AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Crash at reading returnAddress: {e.ToString()}", this); }

                    isInjectionUsed = false;
                    return returnBytes.ToArray();
                }
            }
            catch { }
            isInjectionUsed = false;
            return new List<byte>().ToArray();
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