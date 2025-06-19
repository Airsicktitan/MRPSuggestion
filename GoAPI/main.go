package main

import (
	"encoding/json"
	"fmt"
	"net/http"
)

type Recommendation struct {
	Material          string `json:"material"`
	MaterialType      string `json:"materialType"`
	ProcurementType   string `json:"procurementType"`
	ForecastIndicator string `json:"forecastIndicator"`
	Plant             string `json:"plant"`
	CurrentMrp        string `json:"currentMrp"`
	RecommendedMrp    string `json:"recommendedMrp"`
}

func main() {
	resp, err := http.Get("http://localhost:5000/recommendations")
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var recs []Recommendation
	if err := json.NewDecoder(resp.Body).Decode(&recs); err != nil {
		panic(err)
	}

	for _, rec := range recs {
		fmt.Printf("Material: %s | MRP: %s → %s\n", rec.Material, rec.CurrentMrp, rec.RecommendedMrp)
	}
}
