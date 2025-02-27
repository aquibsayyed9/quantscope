import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Layout from './components/layout/Layout'
import Dashboard from './pages/Dashboard'
import Explorer from './pages/Explorer'
import Connectors from './pages/Connectors'

function App() {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/explorer" element={<Explorer />} />
          <Route path="/connectors" element={<Connectors />} />
        </Routes>
      </Layout>
    </Router>
  )
}

export default App