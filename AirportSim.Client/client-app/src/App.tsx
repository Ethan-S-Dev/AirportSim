import './App.css';

const App = ()=> {
  

  const connectToControlTower = ()=>{

  }

  return (
      <div className='start-box' onClick={()=>connectToControlTower()}>
        <h1>Welcome!</h1>
        <h2>Press to start...</h2>
      </div>
  );
}

export default App;
