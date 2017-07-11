using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudioIDE
{
    public class Instructions
    {
        public List<Instruction> CurrentInstructions_ = new List<Instruction>();
        private InstructionConstants InstructionConstants_ = new InstructionConstants();

        public int InstCount()
        {
            return CurrentInstructions_.Count;
        }

        public bool AddOp(string Operand, EOperand Type, short Value, Label DisplayLabel)
        {
            if(CurrentInstructions_.Count == 0 || LastInstruction().Finished)
            {
                CurrentInstructions_.Add(new Instruction());
                ScopeVisual.ConsoleOut.AddLine("add new instruction");
            }

            if(Operand == "End")
            {
                int ScopeEnderCount = 0;
                bool CorrectScopeCount = false;

                //Find the correct value for the 'End' operand
                for(int i = CurrentInstructions_.Count - 1 ; i >= 0 ; --i)
                {
                    string InstOp = CurrentInstructions_[i].GetOperationString();

                    if(InstOp == "End")
                    {
                        ++ScopeEnderCount;
                    }
                    else if(InstOp == "If")
                    {
                        if(ScopeEnderCount == 0)
                        {
                            Value = InstructionConstants.EndIf;
                            CorrectScopeCount = true;
                            break;
                        }

                        --ScopeEnderCount;
                    }
                    else if(InstOp == "For")
                    {
                        if(ScopeEnderCount == 0)
                        {
                            Value = InstructionConstants.EndFor;
                            CorrectScopeCount = true;
                            break;
                        }

                        --ScopeEnderCount;
                    }
                }

                if(CorrectScopeCount == false)
                    ScopeVisual.ConsoleOut.AddLine("Instruction 'End' does not belong here, no scope starter (If/For) found before this point");
            }
            
            LastInstruction().AddOperand(Operand, Type, Value, DisplayLabel);

            return LastInstruction().TryFinish(InstructionConstants_);
        }

        public Label RemoveLastOperand()
        {
            Instruction LastInst = LastInstruction();
            Label RemoveLabel = LastInst.RemoveLastOperand();
            
            if(LastInst.OperandCount() == -1)
            {
                //Remove the empty instruction
                CurrentInstructions_.RemoveAt(CurrentInstructions_.Count - 1);
            }

            return RemoveLabel;
        }
        
        public Instruction LastInstruction()
        {
            return CurrentInstructions_.LastOrDefault();
        }

        public List<short> InstructionsValuesWithEnd()
        {
            List<short> Values = new List<short>();

            foreach(Instruction Inst in CurrentInstructions_)
            {
                Values.AddRange(Inst.OperandValues());
            }

            Values.Add(InstructionConstants.EndProgram);

            return Values;
        }
    }
}
