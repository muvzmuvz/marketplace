﻿using Microsoft.Identity.Client;

namespace marketplace_api.ModelsDto;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}
