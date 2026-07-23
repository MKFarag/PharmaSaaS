namespace PharmaSaaS.Application.Mapping;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region UserResponse

        config.NewConfig<(User user, IList<string> roles), UserResponse>()
            .Map(dest => dest, src => src.user)
            .Map(dest => dest.Roles, src => src.roles);

        #endregion

        #region ShiftResponse

        config.NewConfig<Shift, ShiftResponse>()
            .Map(dest => dest.CurrentBalance, src => src.OpeningBalance + src.TotalSales - src.TotalReturns - src.TotalExpenses);

        #endregion
    }
}
