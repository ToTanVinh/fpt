import React, { useEffect, useState } from "react";
import axios from "axios";
import { Pannellum } from "pannellum-react";
import "../style/PanoramaView.css";
import HotspotSlider from "./HotspotSlider";

const PanoramaViewer = () => {
  const [panoramaData, setPanoramaData] = useState(null);
  const [currentScene, setCurrentScene] = useState(null);
  const [imageBase64, setImageBase64] = useState(null);
  const [hotspots, setHotSpots] = useState([]);

  useEffect(() => {
    axios
      .get("https://localhost:7059/api/panorama/panoramas")
      .then((response) => {
        const data = response.data;
        setPanoramaData(data);
        setCurrentScene(data.defaultData.firstScene || "scene1");
      })
      .catch((error) => {
        console.error("Error fetching panorama data:", error);
      });
  }, []);

  useEffect(() => {
    if (panoramaData && currentScene) {
      const imageUrl = panoramaData.scenes[currentScene].panorama;
      setImageBase64(null);
      if (imageUrl) {
        loadPanoramaImage(imageUrl);
      }
    }
    return () => {
      setImageBase64(null);
    };
  }, [panoramaData, currentScene]);

  useEffect(() => {
    if (panoramaData && currentScene) {
      const currentSceneData = panoramaData.scenes[currentScene];
      setHotSpots(currentSceneData.hotSpots || []);
    }
  }, [panoramaData, currentScene]);

  const loadPanoramaImage = (imageUrl) => {
    fetch(imageUrl)
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.blob();
      })
      .then((blob) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          setImageBase64(reader.result);
        };
        reader.readAsDataURL(blob);
      })
      .catch((error) => {
        console.error("Error loading image:", error);
      });
  };

  const handleHotspotClick = (sceneIdTarget, sceneId) => {
    if (sceneIdTarget) {
      setCurrentScene(sceneIdTarget);
    } else if (sceneId) {
      setCurrentScene(sceneId);
    }
  };

  const handleHotspotUpdate = () => {
    axios
      .get(`https://localhost:7059/api/panorama/${currentScene}/hotspot`)
      .then((response) => {
        setHotSpots(response.data);
        console.log("Danh sách hotspot đã được cập nhật:", response.data);
      })
      .catch((error) => {
        console.error("Không thể tải lại danh sách hotspot:", error);
      });
  };

  const handleHotspotMove = (hotspotId, newPitch, newYaw) => {
    setHotSpots((prevHotspots) =>
      prevHotspots.map((hotspot) =>
        hotspot.hotspotId === hotspotId
          ? { ...hotspot, pitch: newPitch, yaw: newYaw }
          : hotspot
      )
    );
  };

  if (!panoramaData || !currentScene || !imageBase64) {
    return <p>Loading...</p>;
  }

  const currentSceneData = panoramaData.scenes[currentScene];
  return (
    <div>
      <Pannellum
        width="100vw"
        height="100vh"
        image={imageBase64}
        pitch={currentSceneData.pitch || 0}
        yaw={currentSceneData.yaw || 0}
        hfov={currentSceneData.hfov || 110}
        autoLoad={panoramaData.defaultData.autoLoad}
        autoRotate={panoramaData.defaultData.autoRotate}
        showControls={true}
        onLoad={() => {
          console.log(`Loaded scene: ${currentScene}`);
          console.log("Hotspots: ", currentSceneData.hotSpots);
        }}
      >
        {hotspots && Array.isArray(hotspots) ? (
          hotspots.map((hotspot) => (
            <Pannellum.Hotspot
              key={hotspot.hotspotId}
              type={"custom"}
              pitch={hotspot.pitch}
              yaw={hotspot.yaw}
              text={hotspot.text}
              URL={hotspot.url}
              cssClass={`custom-hotspot ${hotspot.type}-hotspot`}
              handleClick={() => {
                if (hotspot.type === "scene") {
                  handleHotspotClick(hotspot.sceneIdTarget, hotspot.sceneId);
                } else if (hotspot.type === "image") {
                  console.log("Image hotspot clicked: ", hotspot.url);
                } else if (hotspot.type === "info") {
                  console.log("Info hotspot clicked: ", hotspot.text);
                }
              }}
            />
          ))
        ) : (
          <p>No hotspots available</p>
        )}
      </Pannellum>
      <HotspotSlider
        sceneId={currentScene}
        hotspots={hotspots}
        onHotSpotUpdate={handleHotspotUpdate}
      />
    </div>
  );
};

export default PanoramaViewer;
