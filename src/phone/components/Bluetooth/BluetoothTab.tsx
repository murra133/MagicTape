import { useEffect, useContext, useState, useCallback, useRef } from 'react';
import {View,Text,StyleSheet,Pressable, AnimatableNumericValue} from 'react-native';
import {BLEContext} from './BLEContext';
import { WebsocketContext } from '../Websocket/WebsocketContext';
import { Device } from 'react-native-ble-plx';
import { Gyroscope, Accelerometer } from 'expo-sensors';



export function BluetoothTab(){
    const {addCallbacktoMapBLE,requestPermissions, scanForDevices, foundDevices, scanning, connectToDevice, connectedDevice} = useContext(BLEContext);
    const [deviceAttempt, setDeviceAttempt] = useState<Device | null>(null);
    const [permissions, setPermissions] = useState<boolean>(false);
    const {send,isReady} = useContext(WebsocketContext);
    const sensorData = useRef<number[]>([0,0,0,0,0,0]);

// length
// buttonPressed
// RotationX
// RotationY
// RotationZ
// Timestamp
// Location (edited) 
    // setup Gyroscope
    useEffect(()=>{
       
        Gyroscope.setUpdateInterval(5000);
        Gyroscope.addListener((data)=>{
            sensorData.current[0] = data.x;
            sensorData.current[1] = data.y;
            sensorData.current[2] = data.z;
        }
        )
        Accelerometer.setUpdateInterval(5000);
        Accelerometer.addListener((data)=>{
            // generate timestamp
            sensorData.current[3] = data.x;
            sensorData.current[4] = data.y;
            sensorData.current[5] = data.z;
            let message = {
                action : 'messageRouting',
                rx : sensorData.current[0],
                ry : sensorData.current[1],
                rz : sensorData.current[2],
                ax : sensorData.current[3],
                ay : sensorData.current[4],
                az : sensorData.current[5],
                id : "JamesTang",
                timestamp : new Date().getTime(),
            }

            if(connectedDevice){
                connectedDevice.localName = connectedDevice.localName;
            }
            send(message);
        }
        )
    },[])


    useEffect(()=>{
        addCallbacktoMapBLE(0,sendMeasurement);
        addCallbacktoMapBLE(1,sendMeasurementLive);

    },[])

    useEffect(()=>{
        addCallbacktoMapBLE(0,sendMeasurement);
        addCallbacktoMapBLE(1,sendMeasurementLive);
    },[isReady])

    const Scan = async () => {
        const permission = await requestPermissions();
        if(permission){
            scanForDevices();
            setPermissions(true);
        }
    }

    const sendMeasurementLive = useCallback((measurement : number) => {
        let message = {
            action : 'messageRouting',
            distance : measurement,
            locked : false,
            id : 'JamesTang',
            timeStamp : new Date().getTime()
        }
        send(message);
    },[isReady])
    
    const sendMeasurement = useCallback((measurement : number) => {
        let message = {
            action : 'messageRouting',
            distance : measurement,
            locked : true,
            id : 'JamesTang',
            timeStamp : new Date().getTime()
        }
        send(message);
    },[isReady])

    useEffect(()=>{
    },[foundDevices])

    useEffect(()=>{
        if(connectedDevice){
            setDeviceAttempt(null);
        }
    },[connectedDevice])

    const handleDevicePress = async (device : Device) => {
        setDeviceAttempt(device);
      await connectToDevice(device);
    }

    return(
        <View style= {styles.container}>
            <Pressable style={styles.button} onPress={Scan}><Text style={styles.TextColor}>Scan</Text></Pressable>
            {foundDevices.map((device)=>{
                return(
                    <Pressable key={device.id} style={connectedDevice?.id==device.id?styles.button:styles.buttonSelected} onPress={()=>handleDevicePress(device)}>
                        <Text style={styles.TextColor}>{device.name}</Text>
                    </Pressable>
                )
            })}
        </View>
    )
    
}

export default BluetoothTab;

const styles = StyleSheet.create({
    container :{
        flex:1,
        display:'flex',
        justifyContent:'center',
        alignItems:'center',
        backgroundColor:'white',
    },
    button:{
        backgroundColor:'blue',
        color:'white',
        
        padding:10,
        borderRadius:5,
    },
    buttonSelected:{
        backgroundColor:'green',
        color:'white',
        padding:10,
        borderRadius:5, 
    },
    TextColor :{
        color:'white',
    }
  });