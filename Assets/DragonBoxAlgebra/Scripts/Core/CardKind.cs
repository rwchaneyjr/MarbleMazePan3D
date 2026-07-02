namespace DragonBoxAlgebra.Core
{
    public enum CardKind
    {
        Box,
        DayCreature,
        NightCreature,
        PositiveConstant,
        NegativeConstant,
        One,
        DivideTool
    }

    public enum CombineActionType
    {
        OppositeCancel,
        MergeToOne,
        OneEliminates,
        DividePair
    }
}
