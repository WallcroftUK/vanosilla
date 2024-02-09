﻿// WingsEmu
// 
// Developed by NosWings Team

using System;

namespace PhoenixLib.DAL
{
    public interface IGenericAsyncUuidRepository<T> : IGenericAsyncRepository<T, Guid> where T : class, IUuidDto
    {
    }
}