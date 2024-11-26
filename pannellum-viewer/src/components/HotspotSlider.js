import axios from "axios";
import React, { useEffect, useState } from "react";
import "../style/HotspotSlider.css";
import Modal from "react-modal";

const HotspotSlider = ({ sceneId, hotspots, onHotSpotUpdate }) => {
  const [selectedHotspot, setSelectedHotspot] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAddMode, setIsAddMode] = useState(false);
  const [formData, setFormData] = useState({
    pitch: 0,
    yaw: 0,
    type: "",
    text: "",
    sceneIdTarget: "",
    url: "",
    target: "",
    image: "",
    width: "",
    height: "",
    hotspotIdentifier: "",
  });

  useEffect(() => {
    console.log("Hotspots in parent:", hotspots);
    if (hotspots && hotspots.length > 0 && !selectedHotspot) {
      setSelectedHotspot(hotspots[0]);
    }
  }, [hotspots]);

  useEffect(() => {
    if (selectedHotspot) {
      console.log("Selected Hotspot changed:", selectedHotspot);
      setFormData(selectedHotspot);
    }
  }, [selectedHotspot]);

  const handleHotspotChange = (event) => {
    console.log("Selected hotspot ID:", event.target.value);
    const hotspotId = Number(event.target.value);
    const selected = hotspots.find(
      (hotspot) => hotspot.hotspotId === hotspotId
    );
    if (selected) {
      setSelectedHotspot(selected);
      setFormData((prevFormData) => ({
        ...prevFormData,
        ...selected,
      }));
    } else {
      console.error("Hotspot not found");
    }

    console.log("Hotspots in dropdown:", hotspots);
    console.log("Selected hotspot ID in change handler:", event.target.value);
  };

  const handleUpdateClick = () => {
    if (!selectedHotspot) {
      console.error("No hotspot selected");
      return;
    }
    setIsAddMode(false);
    setIsModalOpen(true);
  };

  const handleAddClick = () => {
    setFormData({
      pitch: 0,
      yaw: 0,
      type: "",
      text: "",
      sceneIdTarget: "",
      url: "",
      target: "",
      image: "",
      width: "",
      height: "",
      hotspotIdentifier: "",
    });
    setIsAddMode(true);
    setIsModalOpen(true);
  };

  const handleFormSubmit = () => {
    console.log("Scene ID:", sceneId);
    console.log("Form Data:", formData);

    if (!selectedHotspot || !selectedHotspot.hotspotId) {
      console.error("Selected hotspot or hotspot ID is missing");
      return;
    }

    const apiUrl = isAddMode
      ? `https://localhost:7059/api/panorama/${sceneId}/hotspot`
      : `https://localhost:7059/api/panorama/${sceneId}/hotspot/${selectedHotspot.hotspotId}`;

    const requestMethod = isAddMode ? axios.post : axios.put;

    requestMethod(apiUrl, formData)
      .then((response) => {
        console.log(
          isAddMode ? "Hotspot added: " : "Hotspot updated:",
          response.data
        );
        onHotSpotUpdate(response.data);
        setIsAddMode(false);
        setIsModalOpen(false);
        alert(
          isAddMode
            ? "Hotspot đã được thêm thành công!"
            : "Hotspot đã được cập nhật!"
        );
      })
      .catch((error) => {
        console.error("Error submiting  hotspot:", error);
      });
  };

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  return (
    <div className="hotspot-slider">
      <h3>Select a hotspot</h3>
      <select
        onChange={handleHotspotChange}
        value={selectedHotspot ? selectedHotspot.hotspotId : ""}
        className="dropdown-box"
      >
        {Array.isArray(hotspots) &&
          hotspots.map((hotspot) => {
            return (
              <option key={hotspot.hotspotId} value={hotspot.hotspotId}>
                {`Hotspot ${hotspot.text}`}
              </option>
            );
          })}
      </select>

      <div>
        <button onClick={handleUpdateClick}>Update Hotspots</button>
        <button onClick={handleAddClick}>Add Hotspot</button>
      </div>

      <Modal
        isOpen={isModalOpen}
        onRequestClose={() => setIsModalOpen(false)}
        contentLabel="Hotspot Modal"
        className="hotspot-modal"
        overlayClassName="hotspot-overlay"
      >
        <h2>{isAddMode ? "Add Hotspot" : "Update Hotspot"}</h2>
        <form>
          {Object.keys(formData).map((key) => (
            <div key={key} className="form-group">
              <label>{key}</label>
              <input
                type="text"
                name={key}
                value={formData[key]}
                onChange={handleInputChange}
              />
            </div>
          ))}
        </form>
        <button onClick={handleFormSubmit}>
          {isAddMode ? "Add" : "Update"}
        </button>
        <button onClick={() => setIsModalOpen(false)}>Cancel</button>
      </Modal>
    </div>
  );
};

export default React.memo(HotspotSlider);
