# **Muon Detector Reader**  
**Muon Detector Reader** is a **C#** software that analyzes data produced by [Ekar-Muon-Detector](https://github.com/Marco-Parisi/Ekar-Muon-Detector), correcting them based on atmospheric pressure and temperature. The software generates various interactive graphs to study the trend of muon flux over time.  

## **Features**  
- Reads **TXT** data files from the muon detector  
- Corrects raw counts based on pressure and temperature  
- Generates interactive graphs to visualize muon trends over time  
- Allows saving graphs as **PNG**  

## **Data Format**  
The software reads **text files (.txt)** with the following format:  

```
YYYY-MM-DD HH:MM:SS * Temperature (°C) * Pressure (hPa) *  Counts
```
- Example :
```
2022/08/05 13:44:37 * 35.80 * 979.26 * 8816
2022/08/05 14:44:37 * 36.40 * 978.79 * 8719
2022/08/05 15:44:37 * 36.80 * 978.43 * 8833
2022/08/05 16:44:37 * 37.00 * 978.22 * 8892
```

## **Software Output**  
The processed data is displayed as interactive graphs, which can be saved as **PNG** files.  

<div>
  <p align="center">
    <font size=2><i>Pressure Corrected Counts</i></font>
  </p>
  <p align="center">
    <img src="https://github.com/user-attachments/assets/8fbcaa81-e701-410c-b9fd-b116b9c27102" width="700"/>
  </p>
</div>

<div>
  <p align="center">
    <font size=2><i>Pressure vs Raw Counts</i></font>
  </p>
  <p align="center">
    <img src="https://github.com/user-attachments/assets/e82debce-d1be-4fab-b495-eaf168e29955" width="700"/>
  </p>
</div>

<div>  
  <p align="center">
    <font size=2><i>Barometric Coefficient</i></font>
  </p>
  <p align="center">
    <img src="https://github.com/user-attachments/assets/f15b3d35-f2d0-4fc4-81ea-368b284459cc" width="700"/>
  </p>
</div>

<div>
  <p align="center">
    <font size=2><i>Sigma Counts</i></font>
  </p>
  <p align="center">
    <img src="https://github.com/user-attachments/assets/bc2a19bc-a04b-4377-98c1-7a6514426742" width="700"/>
  </p>
</div>
