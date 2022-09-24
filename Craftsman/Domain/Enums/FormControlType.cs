namespace Craftsman.Domain.Enums;

using Ardalis.SmartEnum;

public abstract class FormControlType : SmartEnum<FormControlType>
{
    public static readonly FormControlType Default = new DefaultType();
    public static readonly FormControlType TextArea = new TextAreaType();
    public static readonly FormControlType Combobox = new ComboboxType();

    protected FormControlType(string name, int value) : base(name, value)
    {
    }
    
    private class DefaultType : FormControlType
    {
        public DefaultType() : base(nameof(Default), 1) { }
    }

    private class TextAreaType : FormControlType
    {
        public TextAreaType() : base(nameof(TextArea), 2) { }
    }

    private class ComboboxType : FormControlType
    {
        public ComboboxType() : base(nameof(Combobox), 3) { }
    }
}
