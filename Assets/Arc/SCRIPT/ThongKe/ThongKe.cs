using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThongKe : MonoBehaviour
{
    public int soLanVaTuong = 0;

    // Số lần chết theo từng chỉ số
    public int soLanChetDoDoi = 0;
    public int soLanChetDoKhat = 0;
    public int soLanChetDoMet = 0;
    public int soLanChetDoStress = 0;

    public int soLanAnDung = 0;
    public int soLanUongDung = 0;
    public int soLanNguDung = 0;
    public int soLanLamViecDung = 0;
    public int soLanGiaiTriDung = 0;

    public int soLanChet = 0;
    // Số step đi được trước khi chết
    public int countStepTruocKhiChetToiDa = 0;
    public int countStepTruocKhiChetToiThieu = 9999;
    public float countStepChetTrungBinh;
    private float tongChet;
    public GridBrain gridBrain;
    private int soLanDungImDung;

    // Gọi khi agent va vào tường hoặc đi sai ô
    public void ThemLanVaTuong()
    {
        soLanVaTuong++;
    }

    // Gọi khi agent "chết", xác định nguyên nhân
    public void ThemLanChet()
    {
        if (gridBrain.character.Food <= 0f) soLanChetDoDoi++;
        if (gridBrain.character.Drink <= 0f) soLanChetDoKhat++;
        if (gridBrain.character.Sleep <= 0f) soLanChetDoMet++;
        if (gridBrain.character.Stress >= 72f) soLanChetDoStress++;
        soLanChet++;
        // Lưu số step lúc chết
        if (gridBrain.count >= countStepTruocKhiChetToiDa)
        {
            countStepTruocKhiChetToiDa = gridBrain.count;
        }
        if (gridBrain.count <= countStepTruocKhiChetToiThieu)
        {
            countStepTruocKhiChetToiThieu = gridBrain.count;
        }
        tongChet += gridBrain.count;
        countStepChetTrungBinh = tongChet / soLanChet;
    }

    // Reset thống kê khi bắt đầu episode mới
    public void ResetThongKe()
    {
        soLanVaTuong = 0;
        soLanChetDoDoi = 0;
        soLanChetDoKhat = 0;
        soLanChetDoMet = 0;
        soLanChetDoStress = 0;
        soLanAnDung = 0;
        soLanUongDung = 0;
        soLanNguDung = 0;
        soLanLamViecDung = 0;
        soLanGiaiTriDung = 0;
        countStepTruocKhiChetToiDa = 0;
        countStepTruocKhiChetToiThieu = 0;
        countStepChetTrungBinh = 0;
        soLanChet = 0;
        soLanDungImDung = 0;
    }

    // Có thể gọi mỗi frame để cập nhật info (debug/hiển thị)
    public void ThemLanThucHienDungAction(int action)
    {
        switch (action)
        {
            case 0: soLanDungImDung++; break;        // Eating
            case 1: soLanAnDung++; break;        // Eating
            case 2: soLanUongDung++; break;      // Drinking
            case 3: soLanNguDung++; break;       // Sleeping
            case 4: soLanLamViecDung++; break;   // Working
            case 5: soLanGiaiTriDung++; break;   // Relaxing
        }
    }
}
