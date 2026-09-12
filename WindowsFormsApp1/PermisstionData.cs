using System;

namespace WindowsFormsApp1
{
    internal class PermisstionData
    {
        public Guid Id { get; internal set; }
        public int RoleId { get; internal set; }
        public string PrincipalDataId { get; internal set; }
        public int OperationId { get; internal set; }
        public int Permisstion { get; internal set; }
        public object DataType { get; internal set; }
        public string DataFiledId { get; internal set; }
    }
}