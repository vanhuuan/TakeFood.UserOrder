using StoreService.Model.Entities.Store;
using StoreService.Model.Entities.Voucher;
using StoreService.Model.Repository;
using TakeFood.UserOrderService.ViewModel.Dtos.Voucher;

namespace TakeFood.UserOrderService.Service.Implement;

public class VouchersService : IVoucherService
{
    private IMongoRepository<Voucher> voucherRepository { get; set; }

    private IMongoRepository<Store> storeRepository { get; set; }

    public VouchersService(IMongoRepository<Voucher> voucherRepository, IMongoRepository<Store> storeRepository)
    {
        this.voucherRepository = voucherRepository;
        this.storeRepository = storeRepository;
    }
    public async Task CreateVoucherAsync(CreateVoucherDto dto, string ownerId)
    {
        var store = await storeRepository.FindByIdAsync(dto.StoreId);
        if (store == null || store.OwnerId != ownerId)
        {
            throw new Exception("You're not the owner or store not exist");
        }

        var checkCode = await voucherRepository.FindOneAsync(x => x.Code == dto.Code);
        if (checkCode != null)
        {
            throw new Exception("Code existed");
        }

        if (dto.StartDay < DateTime.Now || dto.ExpireDay < DateTime.Now || dto.StartDay < dto.StartDay)
        {
            throw new Exception("Unexecpted Datetime");
        }

        if (dto.Amount <= 0 || dto.MaxDiscount <= 0 || dto.MinSpend < 0)
        {
            throw new Exception("Money can't not be negative");
        }

        var voucher = new Voucher()
        {
            Amount = dto.Amount,
            StoreId = store.Id,
            StartDay = dto.StartDay,
            ExpireDay = dto.ExpireDay,
            Code = dto.Code,
            Description = dto.Description,
            MaxDiscount = dto.MaxDiscount,
            MinSpend = dto.MinSpend,
            Name = dto.Name,
            Type = false
        };

        await voucherRepository.InsertAsync(voucher);
    }

    public async Task<List<VoucherCardDto>> GetAllStoreVoucherOkeAsync(string storeId)
    {
        var now = DateTime.Now;
        var listVoucher = await voucherRepository.FindAsync(x => (x.StoreId == storeId && x.StartDay <= now && x.ExpireDay <= now) || x.Type == true); // Lay voucher cua cua hang hoac voucher cua he thong
        var rs = new List<VoucherCardDto>();
        foreach (var voucher in listVoucher)
        {
            rs.Add(new VoucherCardDto()
            {
                MaxDiscount = voucher.MaxDiscount,
                MinSpend = voucher.MinSpend,
                Amount = voucher.Amount,
                Description = voucher.Description,
                Name = voucher.Name,
            });
        }
        return rs;
    }

    public async Task UpdateVoucherAsync(UpdateVoucherDto dto, string ownerId)
    {
        var store = await storeRepository.FindByIdAsync(dto.StoreId);
        if (store == null || store.OwnerId != ownerId)
        {
            throw new Exception("You're not the owner or store not exist");
        }

        if (dto.StartDay < DateTime.Now || dto.ExpireDay < DateTime.Now || dto.StartDay < dto.StartDay)
        {
            throw new Exception("Unexecpted Datetime");
        }

        if (dto.Amount <= 0 || dto.MaxDiscount <= 0 || dto.MinSpend < 0)
        {
            throw new Exception("Money can't not be negative");
        }
        var voucher = await voucherRepository.FindByIdAsync(dto.VoucherId);
        if (voucher != null)
        {
            voucher.Amount = dto.Amount;
            voucher.StoreId = store.Id;
            voucher.StartDay = dto.StartDay;
            voucher.ExpireDay = dto.ExpireDay;
            voucher.Code = dto.Code;
            voucher.Description = dto.Description;
            voucher.MaxDiscount = dto.MaxDiscount;
            voucher.MinSpend = dto.MinSpend;

            await voucherRepository.UpdateAsync(voucher);
        }
        else
        {
            throw new Exception("Voucher is not exist");
        }
    }
}
