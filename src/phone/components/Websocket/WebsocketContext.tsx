import React, { createContext, useRef, useEffect, useState, ReactNode } from "react";
import { AppState } from "react-native";

interface WebSocketContextType {
    isReady: boolean;
    attemptingConnection: boolean;
    send: (message: Message, callback? : (reply : string)=>void) => void;
    webSocketDisconnect: () => void;
    webSocketReconnect: () => void;
}

interface Message {
    [key: string]: any; // Allow any property
    messageid?: number; // Optional messageid property
}


export const WebsocketContext = createContext<WebSocketContextType>({
    isReady: false,
    attemptingConnection: false,
    send: () => {},
    webSocketDisconnect: () => {},
    webSocketReconnect: () => {},
});

const WebsocketProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [appState, setAppState]  = useState<string>(AppState.currentState);
    const [isReady, setIsReady] = useState(false);
    const [attemptingConnection, setAttemptingConnection] = useState(false);
    const [ws, setWs] = useState<WebSocket | null>(null);
    const messageQueue = useRef<string[]>([]);
    const messageMap = useRef<Map<number, (reply : string)=>void>>(new Map<number, (reply : string)=>void>());
    const receivedMessage = useRef<Object | null>(null);
    const messageId = useRef<number>(100);
    
    const WSURL = useRef<String>("wss://nsh4jydg7e.execute-api.us-west-1.amazonaws.com/production/");
    
    useEffect(()=>{
        const interval = setInterval(() => {
            if (messageQueue.current.length > 0) {
                if(ws && ws.readyState === 1){
                    const message = messageQueue.current[0];
                    console.log('sending message', message);
                    ws.send(message);
                    messageQueue.current.shift();

                }
                else if(ws && ws.readyState === 3){
                    webSocketReconnect();
                }
            }
            }, 50);
            return () => clearInterval(interval)
        },[ws]);
    
    useEffect(()=>{
        if ((ws?.readyState==3 || !ws) && appState === 'active') {
            webSocketReconnect();
        }
    },[appState, ws]);

    useEffect(() => {
        const handleAppStateChange = (nextAppState: string) => {
          setAppState(nextAppState);
        };
        const subscription = AppState.addEventListener("change", handleAppStateChange);
        return () => {
            subscription.remove();
        };
    }, []);


    const handleMessage = (data: any) => {
        // build the receive action
};


    const send = (message:Message, callback? : (reply : string)=> void) => {
        if (callback){
            message.messageId = messageId.current;
            messageMap.current.set(messageId.current,callback);
            messageId.current++;
        }
        messageQueue.current.push(JSON.stringify(message));
    };
    const webSocketDisconnect = () => {
        if (ws) {
            ws.close();
        }
    };

    const webSocketReconnect = () => {
        setAttemptingConnection(true);
        ws?.close();
        const websocket = new WebSocket(WSURL.current.toString());
        websocket.onopen = () => {
            setIsReady(true);
            setAttemptingConnection(false);
        };

        websocket.onclose = () => {
            setIsReady(false);
            setAttemptingConnection(false);
        };

        websocket.onmessage = (event) => {
            let data = JSON.parse(event.data);
            handleMessage(data);
        };
        setWs(websocket);
    };

    const contextValue: WebSocketContextType = {
        isReady,
        send,
        webSocketDisconnect,
        webSocketReconnect,
        attemptingConnection,
    };

    return (
        <WebsocketContext.Provider value={contextValue}>
            {children}
        </WebsocketContext.Provider>
    );
};

export default WebsocketProvider;
