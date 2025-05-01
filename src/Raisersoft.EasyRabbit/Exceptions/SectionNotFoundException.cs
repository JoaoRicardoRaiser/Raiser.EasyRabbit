using System;

namespace Raisersoft.EasyRabbit.Exceptions;

public class SectionNotFoundException(string sectionKey) 
    : Exception($"Section {sectionKey} not present in configuration.") {}
