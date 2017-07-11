using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudioIDE
{
    public enum EOperand
    {
        Variable,
        Immediate,
        Operation
    }

    public struct Operand
    {
        public Operand(string Op, EOperand Type, short Value, Label DisplayLabel)
        {
            this.OP = Op;
            this.Type = Type;
            this.Value = Value;
            this.DisplayLabel = DisplayLabel;
        }

        public string OP;
        public EOperand Type;
        public short Value;
        public Label DisplayLabel;
    }

    public class Instruction
    {
        private List<Operand> Operands_ = new List<Operand>();
        private string Operation_ = null;
        private bool Finished_ = false;
        public bool Finished { get { return Finished_; } }

        public void AddOperand(string Operand, EOperand Type, short Value, Label DisplayLabel)
        {
            Operand NewOperand = new Operand(Operand, Type, Value, DisplayLabel);
            
            Operands_.Add(NewOperand);

            if(Type == EOperand.Operation)
                Operation_ = Operand;
        }

        //Returns the label of the operand if an operand was removed. Returns null otherwise
        public Label RemoveLastOperand()
        {
            Finished_ = false;

            if(Operands_.Count > 0)
            {
                Label RemoveLabel = Operands_.Last().DisplayLabel;
                Operands_.RemoveAt(Operands_.Count - 1);
                return RemoveLabel;
            }
            
            return null;
        }

        public bool TryGetOperation(out Operand OutOperand)
        {
            foreach(Operand Op in Operands_)
            {
                if(Op.Type == EOperand.Operation)
                {
                    OutOperand = Op;
                    return true;
                }
            }

            OutOperand = new Operand();
            return false;
        }

        public string GetOperationString()
        {
            return Operation_;
        }

        public int OperandCount()
        {
            //Minus one to discount the operation operand
            return Operands_.Count - 1;
        }

        public short[] OperandValues()
        {
            short[] Values = new short[Operands_.Count];

            for(int i = 0 ; i < Values.Count() ; ++i)
            {
                Values[i] = Operands_[i].Value;
            }

            return Values;
        }

        private List<EOperand> OperandTypes()
        {
            List<EOperand> Types = new List<EOperand>();

            foreach(Operand Op in Operands_)
            {
                Types.Add(Op.Type);
            }

            return Types;
        }

        public Label[] DisplayLabels()
        {
            Label[] Labels = new Label[Operands_.Count];
            for(int i = 0 ; i < Operands_.Count ; ++i)
            {
                Labels[i] = Operands_[i].DisplayLabel;
            }

            return Labels;
        }

        //Returns true if the instruction completes
        public bool TryFinish(InstructionConstants InstConsts)
        {
            //Get the value for the operation of this instruction
            short OperationValue;
            int OperandCount = Operands_.Count - 1; //Minus one to discount the operation operand e.g. += (addimm)
            if(InstConsts.TryGetOperationValue(Operation_, OperandCount, out OperationValue))
            {
                for(int i = 0 ; i < Operands_.Count ; ++i)
                {
                    if(Operands_[i].Type == EOperand.Operation)
                    {
                        //Set the value for the operation
                        Operand OpWithoutValue = Operands_[i];
                        OpWithoutValue.Value = OperationValue;
                        Operands_[i] = OpWithoutValue;
                        break;
                    }
                }

                //Check all the operands are correct/valid/in the right place
                if(InstConsts.ValidInstruction(OperationValue, OperandTypes()))
                {
                    List<short> Values = new List<short>(4); //Init capacity 4, most instructions have about 4 
                    List<byte> Indexes = InstConsts.CompilerOrderedInstructionIndexes(OperationValue);
                    foreach(byte Index in Indexes)
                    {
                        Values.Add(Operands_[Index].Value);
                    }

                    for(int i = 0 ; i < Operands_.Count ; ++i)
                    {
                        Operand Op = Operands_[i];
                        Op.Value = Values[i];
                        Operands_[i] = Op;
                    }

                    Finished_ = true;
                }
            }
            
            return Finished_;
        }
    }
}
