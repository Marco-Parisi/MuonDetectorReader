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

### *Pressure Corrected Counts*
![Pressure Corrected Counts](https://github.com/user-attachments/assets/cda7324b-0e37-42cc-a00b-afe31d2528a2)
### *Pressure vs Raw Counts*
![Pressure vs Raw Counts](https://github.com/user-attachments/assets/3b06ba92-5800-43f9-ab18-86ef6b84d57e) 
### *Barometric Coefficient*
![Barometric Coefficient](https://github.com/user-attachments/assets/38554c49-8a4e-4074-a828-700423eb1cf2) 
### *Sigma Counts*
![Sigma Counts](https://github.com/user-attachments/assets/798fad72-40cf-4afe-a0e4-0e35bc1e0d2a) 

