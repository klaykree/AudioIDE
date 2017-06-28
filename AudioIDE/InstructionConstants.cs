using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioIDE
{
    public class InstructionConstants
    {
        //Default instruction values
        private const short AddImm = 4093;
        private const short SubImm = 4094;
        private const short MulImm = 4095;
        private const short DivImm = 4096;
        private const short AddVar = 4097;
        private const short SubVar = 4098;
        private const short MulVar = 4099;
        private const short DivVar = 4100;

        private const short PrintVar = 8191;
        private const short PrintVarLn = 8192;
        private const short PrintLn = 8193;

        private const short CreateVar = 16383;
        private const short AssignFromImmediate = 16384;
        private const short AssignFromVariable = 16385;

        private const short IfImmEql = 24574;
        private const short IfVarEql = 24575;
        private const short ForImm = 24576;
        private const short ForVar = 24577;
        public const short EndIf = 24578;
        public const short EndFor = 24579;
        public const short EndProgram = 24580;

        //Dictionary to look up operand value based on operand (string) and operand count (int)
        private Dictionary<String, Dictionary<int, short>> OperationValues_ = new Dictionary<String, Dictionary<int, short>>();

        //Dictionary of correct IDE operation order e.g. AddVar (short) = variable, operation, variable
        private Dictionary<short, List<EOperand>> IDEOrders_ = new Dictionary<short, List<EOperand>>();

        //Dictionary of correct compiler operation order index based off IDEOrder e.g. AddVar (short) = 1, 0, 2 (operation, variable, variable)
        private Dictionary<short, List<byte>> CompilerOrderIndexes_ = new Dictionary<short, List<byte>>();

        public InstructionConstants()
        {
            SetDefaultValues();
            SetDefaultOrders();
        }

        private void AddOperationValue(string Operation, int OPCount, short Value)
        {
            Dictionary<int, short> SecondDict;
            if(OperationValues_.TryGetValue(Operation, out SecondDict))
            {
                SecondDict.Add(OPCount, Value);
            }
            else
            {
                SecondDict = new Dictionary<int, short>();
                SecondDict.Add(OPCount, Value);
                OperationValues_.Add(Operation, SecondDict);
            }
        }

        public bool TryGetOperationValue(string Operation, int OPCount, out short Value)
        {
            if(Operation != null)
            {
                Dictionary<int, short> SecondDict;
                if(OperationValues_.TryGetValue(Operation, out SecondDict))
                {
                    if(SecondDict.TryGetValue(OPCount, out Value))
                    {
                        return true;
                    }
                }
            }

            Value = 0;
            return false;
        }

        public void SetDefaultValues()
        {
            AddOperationValue("+=", 3, AddImm);
            AddOperationValue("-=", 3, SubImm);
            AddOperationValue("*=", 3, MulImm);
            AddOperationValue("/=", 3, DivImm);

            AddOperationValue("+=", 2, AddVar);
            AddOperationValue("-=", 2, SubVar);
            AddOperationValue("*=", 2, MulVar);
            AddOperationValue("/=", 2, DivVar);

            AddOperationValue("Print variable", 1, PrintVar);
            AddOperationValue("Print variable new line", 1, PrintVarLn);
            AddOperationValue("Print new line", 0, PrintLn);

            AddOperationValue("Int", 3, CreateVar);
            AddOperationValue("=", 2, AssignFromVariable);
            AddOperationValue("=", 3, AssignFromImmediate);

            AddOperationValue("If", 3, IfImmEql);
            AddOperationValue("If", 2, IfVarEql);
            AddOperationValue("For", 2, ForImm);
            AddOperationValue("For", 1, ForVar);
        }
        
        private void SetDefaultOrders()
        {
            IDEOrders_.Add(AddImm, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(SubImm, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(MulImm, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(DivImm, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            CompilerOrderIndexes_.Add(AddImm, new List<byte>() { 1, 0, 2, 3 });
            CompilerOrderIndexes_.Add(SubImm, new List<byte>() { 1, 0, 2, 3 });
            CompilerOrderIndexes_.Add(MulImm, new List<byte>() { 1, 0, 2, 3 });
            CompilerOrderIndexes_.Add(DivImm, new List<byte>() { 1, 0, 2, 3 });
            IDEOrders_.Add(AddVar, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(SubVar, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(MulVar, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(DivVar, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Variable });
            CompilerOrderIndexes_.Add(AddVar, new List<byte>() { 1, 0, 2 });
            CompilerOrderIndexes_.Add(SubVar, new List<byte>() { 1, 0, 2 });
            CompilerOrderIndexes_.Add(MulVar, new List<byte>() { 1, 0, 2 });
            CompilerOrderIndexes_.Add(DivVar, new List<byte>() { 1, 0, 2 });

            IDEOrders_.Add(PrintVar, new List<EOperand>() { EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(PrintVarLn, new List<EOperand>() { EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(PrintLn, new List<EOperand>() { EOperand.Operation });
            CompilerOrderIndexes_.Add(PrintVar, new List<byte>() { 0, 1 });
            CompilerOrderIndexes_.Add(PrintVarLn, new List<byte>() { 0, 1 });
            CompilerOrderIndexes_.Add(PrintLn, new List<byte>() { 0 });

            IDEOrders_.Add(CreateVar, new List<EOperand>() { EOperand.Operation, EOperand.Variable, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(AssignFromVariable, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Variable });
            IDEOrders_.Add(AssignFromImmediate, new List<EOperand>() { EOperand.Variable, EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            CompilerOrderIndexes_.Add(CreateVar, new List<byte>() { 0, 1, 2, 3 });
            CompilerOrderIndexes_.Add(AssignFromVariable, new List<byte>() { 1, 0, 2 });
            CompilerOrderIndexes_.Add(AssignFromImmediate, new List<byte>() { 1, 0, 2, 3 });

            IDEOrders_.Add(IfImmEql, new List<EOperand>() { EOperand.Operation, EOperand.Variable, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(IfVarEql, new List<EOperand>() { EOperand.Operation, EOperand.Variable, EOperand.Variable });
            IDEOrders_.Add(ForImm, new List<EOperand>() { EOperand.Operation, EOperand.Immediate, EOperand.Immediate });
            IDEOrders_.Add(ForVar, new List<EOperand>() { EOperand.Operation, EOperand.Variable });
            CompilerOrderIndexes_.Add(IfImmEql, new List<byte>() { 0, 1, 2, 3 });
            CompilerOrderIndexes_.Add(IfVarEql, new List<byte>() { 0, 1, 2 });
            CompilerOrderIndexes_.Add(ForImm, new List<byte>() { 0, 1, 2 });
            CompilerOrderIndexes_.Add(ForVar, new List<byte>() { 0, 1 });
        }

        public bool ValidInstruction(short InstOperation, List<EOperand> Operands)
        {
            List<EOperand> Order;
            //If the Instruction exists
            if(IDEOrders_.TryGetValue(InstOperation, out Order))
            {
                if(Operands.Count == Order.Count)
                {
                    for(int i = 0 ; i < Operands.Count ; ++i)
                    {
                        if(Operands[i] != Order[i])
                        {
                            return false; //One or more of the operands is incorrect
                        }
                    }

                    return true; //All operands much match up at this point
                }
            }

            return false; //Any other outcome is invalid
        }

        public List<byte> CompilerOrderedInstructionIndexes(short InstOperation)
        {
            return CompilerOrderIndexes_[InstOperation];
        }
    }
}
