﻿namespace marketplace_api;

public class NotFoundExeption : Exception
{
    public NotFoundExeption(string message) : base(message) { }
}
