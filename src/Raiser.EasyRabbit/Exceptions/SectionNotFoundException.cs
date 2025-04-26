using System;

namespace Raiser.EasyRabbit.Exceptions;

public class SectionNotFoundException(string sectionKey) 
    : Exception($"Section {sectionKey} not present in configuration.") {}
